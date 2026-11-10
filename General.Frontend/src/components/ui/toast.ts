import { toast as sonnerToast, type ExternalToast } from 'vue-sonner';

export interface UiToastAction {
  label: string;
  onClick: () => void;
}

export interface UiToastOptions {
  id?: string | number;
  description?: string;
  duration?: number;
  action?: UiToastAction;
}

export interface UiToastPromiseMessages<T> {
  loading: string;
  success: string | ((value: T) => string);
  error: string | ((reason: unknown) => string);
}

function toExternalOptions(options: UiToastOptions): ExternalToast {
  return {
    id: options.id,
    description: options.description,
    duration: options.duration,
    action: options.action
      ? {
          label: options.action.label,
          onClick: () => options.action?.onClick(),
        }
      : undefined,
  };
}

export const toast = {
  success(message: string, options: UiToastOptions = {}) {
    return sonnerToast.success(message, toExternalOptions(options));
  },
  info(message: string, options: UiToastOptions = {}) {
    return sonnerToast.info(message, toExternalOptions(options));
  },
  warning(message: string, options: UiToastOptions = {}) {
    return sonnerToast.warning(message, toExternalOptions(options));
  },
  error(message: string, options: UiToastOptions = {}) {
    return sonnerToast.error(message, toExternalOptions(options));
  },
  loading(message: string, options: UiToastOptions = {}) {
    return sonnerToast.loading(message, toExternalOptions(options));
  },
  dismiss(id?: string | number) {
    return sonnerToast.dismiss(id);
  },
  promise<T>(promise: Promise<T> | (() => Promise<T>), messages: UiToastPromiseMessages<T>) {
    return sonnerToast.promise(promise, {
      loading: messages.loading,
      success: (value: T) => (typeof messages.success === 'function' ? messages.success(value) : messages.success),
      error: (reason: unknown) => (typeof messages.error === 'function' ? messages.error(reason) : messages.error),
    });
  },
};
