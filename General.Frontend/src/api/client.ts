export type ApiQueryValue = string | number | boolean | null | undefined | readonly (string | number | boolean)[];

export interface ApiResponse<T> {
  code: number;
  data: T;
  error: unknown;
  message: string;
}

export interface ApiValidationError {
  message: string;
  members: string[];
}

export interface ApiRequestErrorOptions {
  cause?: unknown;
  code?: number | string;
  details?: string;
  status?: number;
  validationErrors?: ApiValidationError[];
}

export class ApiRequestError extends Error {
  readonly code?: number | string;
  readonly details?: string;
  readonly status?: number;
  readonly validationErrors: ApiValidationError[];

  constructor(message: string, options: ApiRequestErrorOptions = {}) {
    super(message, options.cause === undefined ? undefined : { cause: options.cause });
    this.name = 'ApiRequestError';
    this.code = options.code;
    this.details = options.details;
    this.status = options.status;
    this.validationErrors = options.validationErrors ?? [];
  }
}

export interface ApiClientOptions {
  baseUrl: string;
  fetcher?: typeof fetch;
  getAccessToken?: () => string | null | undefined;
  getSessionVersion?: () => number;
  onUnauthorized?: () => void | Promise<void>;
  timeoutMs?: number;
}

export interface ApiRequestOptions {
  anonymous?: boolean;
  body?: unknown;
  headers?: HeadersInit;
  method?: string;
  query?: URLSearchParams | Readonly<Record<string, ApiQueryValue>>;
  responseType?: 'json' | 'blob' | 'stream';
  signal?: AbortSignal | null;
  timeoutMs?: number;
}

type JsonObject = Record<string, unknown>;

function isJsonObject(value: unknown): value is JsonObject {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function joinApiUrl(baseUrl: string, path: string): string {
  const normalizedBase = baseUrl.trim().replace(/\/+$/u, '');
  const normalizedPath = path.trim().replace(/^\/+/, '');
  if (!normalizedPath || /^https?:\/\//iu.test(path.trim()) || path.trim().startsWith('//')) {
    throw new TypeError('API路径必须是非空的相对路径。');
  }

  return normalizedBase ? `${normalizedBase}/${normalizedPath}` : `/${normalizedPath}`;
}

function appendQuery(url: string, query: ApiRequestOptions['query']): string {
  if (!query) return url;

  const parameters = new URLSearchParams();
  if (query instanceof URLSearchParams) {
    query.forEach((value, key) => parameters.append(key, value));
  } else {
    for (const [key, rawValue] of Object.entries(query)) {
      if (rawValue === null || rawValue === undefined) continue;
      const values = Array.isArray(rawValue) ? rawValue : [rawValue];
      for (const value of values) parameters.append(key, String(value));
    }
  }

  const serialized = parameters.toString();
  if (!serialized) return url;
  return `${url}${url.includes('?') ? '&' : '?'}${serialized}`;
}

function isRawBody(value: unknown): value is BodyInit {
  if (typeof value === 'string') return true;
  if (typeof FormData !== 'undefined' && value instanceof FormData) return true;
  if (typeof Blob !== 'undefined' && value instanceof Blob) return true;
  if (typeof URLSearchParams !== 'undefined' && value instanceof URLSearchParams) return true;
  if (value instanceof ArrayBuffer || ArrayBuffer.isView(value)) return true;
  return false;
}

function fallbackMessage(status?: number): string {
  if (status === undefined) return '网络请求失败，请检查连接后重试。';
  if (status === 400) return '请求参数无效。';
  if (status === 401) return '登录已失效，请重新登录。';
  if (status === 403) return '当前账号无权执行此操作。';
  if (status === 404) return '请求的资源不存在。';
  if (status >= 500) return '服务暂时不可用，请稍后重试。';
  return `请求失败（HTTP ${status}）。`;
}

function readValidationErrors(value: unknown): ApiValidationError[] {
  if (!isJsonObject(value) || !Array.isArray(value.validationErrors)) return [];

  return value.validationErrors.flatMap((item): ApiValidationError[] => {
    if (!isJsonObject(item) || typeof item.message !== 'string' || !item.message.trim()) return [];
    const members = Array.isArray(item.members)
      ? item.members.filter((member): member is string => typeof member === 'string')
      : [];
    return [{ message: item.message, members }];
  });
}

function createResponseError(body: unknown, status: number): ApiRequestError {
  const outer = isJsonObject(body) ? body : undefined;
  const errorValue = outer?.error ?? outer?.Error;
  const remoteError = isJsonObject(errorValue) ? errorValue : undefined;
  const validationErrors = readValidationErrors(remoteError);
  const details = typeof remoteError?.details === 'string' ? remoteError.details : undefined;
  const errorMessage =
    typeof errorValue === 'string'
      ? errorValue
      : typeof remoteError?.message === 'string'
        ? remoteError.message
        : undefined;
  const remoteCode = typeof remoteError?.code === 'string' ? remoteError.code : undefined;
  const businessMessage = remoteCode && /[\u3400-\u9fff]/u.test(remoteCode) ? remoteCode : undefined;
  const hasGenericFrameworkMessage = Boolean(
    errorMessage?.includes('Volo.Abp.BusinessException') ||
    errorMessage === 'Forbidden' ||
    errorMessage === 'An internal error occurred during your request!' ||
    remoteCode?.includes('AbpAuthorizationException'),
  );
  const message =
    (errorMessage && !hasGenericFrameworkMessage ? errorMessage : undefined) ||
    (validationErrors.length > 0 ? validationErrors.map((item) => item.message).join('；') : undefined) ||
    details ||
    (hasGenericFrameworkMessage ? businessMessage : undefined) ||
    (typeof outer?.message === 'string' && outer.message.trim() ? outer.message : undefined) ||
    (hasGenericFrameworkMessage ? undefined : errorMessage) ||
    fallbackMessage(status);
  const code =
    typeof outer?.code === 'number' || typeof outer?.code === 'string'
      ? outer.code
      : remoteCode
        ? remoteCode
        : undefined;

  return new ApiRequestError(message, { code, details, status, validationErrors });
}

function isApiResponse(value: unknown): value is ApiResponse<unknown> {
  return isJsonObject(value) && typeof value.code === 'number' && 'data' in value;
}

function isAbortError(error: unknown): boolean {
  return error instanceof Error && error.name === 'AbortError';
}

function throwTransportError(error: unknown, signal: AbortSignal): never {
  if (
    (signal.aborted && signal.reason instanceof Error && signal.reason.name === 'TimeoutError') ||
    (error instanceof Error && error.name === 'TimeoutError')
  ) {
    throw new ApiRequestError('请求超时，请稍后重试。', { cause: error });
  }
  if (isAbortError(error)) throw error;
  throw new ApiRequestError(fallbackMessage(), { cause: error });
}

export function createApiClient(options: ApiClientOptions) {
  const fetcher = options.fetcher ?? globalThis.fetch;
  let handledUnauthorizedVersion: number | undefined;

  async function request<T>(path: string, requestOptions: ApiRequestOptions = {}): Promise<T> {
    const url = appendQuery(joinApiUrl(options.baseUrl, path), requestOptions.query);
    const headers = new Headers(requestOptions.headers);
    const responseType = requestOptions.responseType ?? 'json';
    const isFormDataBody = typeof FormData !== 'undefined' && requestOptions.body instanceof FormData;
    const requestAccessToken = requestOptions.anonymous ? undefined : options.getAccessToken?.()?.trim();
    const requestSessionVersion = options.getSessionVersion?.();

    if (!headers.has('Accept')) {
      headers.set(
        'Accept',
        responseType === 'blob' ? '*/*' : responseType === 'stream' ? 'application/x-ndjson' : 'application/json',
      );
    }

    if (requestOptions.anonymous) {
      headers.delete('Authorization');
    } else {
      if (requestAccessToken) headers.set('Authorization', `Bearer ${requestAccessToken}`);
    }

    let body: BodyInit | undefined;
    if (requestOptions.body !== undefined) {
      if (isFormDataBody) {
        headers.delete('Content-Type');
      } else if (isRawBody(requestOptions.body)) {
        body = requestOptions.body;
      } else {
        if (!headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
        body = JSON.stringify(requestOptions.body);
      }
      if (isFormDataBody) body = requestOptions.body as FormData;
    }

    let response: Response;
    const timeoutMs =
      requestOptions.timeoutMs ??
      options.timeoutMs ??
      (responseType === 'blob' || responseType === 'stream' || isFormDataBody ? 120_000 : 30_000);
    if (!Number.isSafeInteger(timeoutMs) || timeoutMs <= 0 || timeoutMs > 2147483647)
      throw new TypeError('请求超时必须是1至2147483647之间的整数毫秒。');
    // 超时覆盖响应体读取；页面取消仍通过原始AbortError处理。
    const timeoutSignal = AbortSignal.timeout(timeoutMs);
    const signal = requestOptions.signal ? AbortSignal.any([requestOptions.signal, timeoutSignal]) : timeoutSignal;
    try {
      response = await fetcher(url, {
        body,
        headers,
        method: requestOptions.method ?? 'GET',
        signal,
      });
    } catch (error) {
      throwTransportError(error, signal);
    }

    if (responseType === 'stream' && response.ok) {
      if (!response.body) throw new ApiRequestError('服务端没有返回流式响应体。', { status: response.status });
      return response as T;
    }

    if (responseType === 'blob' && response.ok) {
      try {
        return (response.status === 204 ? undefined : await response.blob()) as T;
      } catch (error) {
        throwTransportError(error, signal);
      }
    }

    let responseText: string;
    try {
      responseText = await response.text();
    } catch (error) {
      throwTransportError(error, signal);
    }
    let responseBody: unknown;
    if (responseText.trim()) {
      try {
        responseBody = JSON.parse(responseText) as unknown;
      } catch {
        if (response.ok) {
          throw new ApiRequestError('服务端返回了无法识别的响应格式。', { status: response.status });
        }
      }
    }

    if (!response.ok) {
      const sessionIsCurrent =
        requestSessionVersion !== undefined && options.getSessionVersion?.() === requestSessionVersion;
      if (
        response.status === 401 &&
        !requestOptions.anonymous &&
        sessionIsCurrent &&
        options.onUnauthorized &&
        handledUnauthorizedVersion !== requestSessionVersion
      ) {
        handledUnauthorizedVersion = requestSessionVersion;
        try {
          await options.onUnauthorized();
        } catch {
          // Keep the original API error as the request result if UI cleanup fails.
        }
      }
      throw createResponseError(responseBody, response.status);
    }
    if (response.status === 204) return undefined as T;
    if (responseBody === undefined) {
      throw new ApiRequestError('服务端返回了空响应。', { status: response.status });
    }
    if (isApiResponse(responseBody)) {
      if (responseBody.code !== 0) throw createResponseError(responseBody, response.status);
      return responseBody.data as T;
    }
    return responseBody as T;
  }

  return { request };
}
