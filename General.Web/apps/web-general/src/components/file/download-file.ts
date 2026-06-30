import { downloadFileApi, type FileApi } from '#/api/core';

export async function downloadPlatformFile(file: FileApi.FileItem) {
  const blob = await downloadFileApi(file.fileKey);
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = file.fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}
