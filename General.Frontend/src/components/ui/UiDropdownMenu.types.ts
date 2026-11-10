export interface UiDropdownMenuItem {
  id: string;
  label: string;
  disabled?: boolean;
  tone?: 'default' | 'danger';
  separatorBefore?: boolean;
  shortcut?: string;
}
