export function useDateFormatter() {
  function formatDateTime(value?: null | string) {
    return value ? new Date(value).toLocaleString() : '-';
  }

  return {
    formatDateTime,
  };
}
