import type { RequestClient } from '../request-client';
import type { RequestClientConfig } from '../types';

import { isUndefined } from '@vben/utils';

class FileUploader {
  private client: RequestClient;

  constructor(client: RequestClient) {
    this.client = client;
  }

  public async upload<T = any>(
    url: string,
    data: Record<string, any> & { file: Blob | File },
    config?: RequestClientConfig,
  ): Promise<T> {
    const formData = new FormData();

    Object.entries(data).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach((item, index) => {
          !isUndefined(item) && formData.append(`${key}[${index}]`, item);
        });
      } else if (value === null) {
        return;
      } else {
        !isUndefined(value) && formData.append(key, value);
      }
    });

    return this.client.post(url, formData, config);
  }
}

export { FileUploader };
