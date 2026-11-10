export interface UiCommandItem {
  id: string;
  label: string;
  description?: string;
  group?: string;
  keywords?: readonly string[];
  shortcut?: string;
  disabled?: boolean;
}
