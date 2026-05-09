import { ref } from 'vue';

export function useActionLoading() {
  const actionLoadingKey = ref('');

  async function runAction<T>(key: string, action: () => Promise<T>): Promise<T> {
    if (actionLoadingKey.value) {
      return undefined as T;
    }

    actionLoadingKey.value = key;
    return action().finally(() => {
      if (actionLoadingKey.value === key) {
        actionLoadingKey.value = '';
      }
    });
  }

  return {
    actionLoadingKey,
    runAction,
  };
}
