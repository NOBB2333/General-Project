import { reactive } from 'vue';

export const accessState = reactive({ codes: [] as string[] });

export function setAccessCodes(codes: readonly string[]): void {
  accessState.codes = [...new Set(codes.filter((code) => code.trim()))];
}

export function clearAccessCodes(): void {
  accessState.codes = [];
}

export function hasAccessCode(code: string): boolean {
  const normalizedCode = code.trim().toLowerCase();
  return accessState.codes.some((value) => value.trim().toLowerCase() === normalizedCode);
}
