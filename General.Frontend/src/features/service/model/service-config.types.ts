export type ServiceConfigTabKey =
  'types' | 'status' | 'warranty' | 'sla' | 'fees' | 'payment' | 'staff' | 'numbering' | 'data';

export type ServiceConfigTone = 'red' | 'green' | 'blue' | 'purple' | 'orange' | 'cyan' | 'gray';

export interface ServiceConfigPanelDefinition {
  key: ServiceConfigTabKey;
  label: string;
  heading: string;
  addLabel: string;
}

export interface ServiceConfigItem {
  id: string;
  tab: ServiceConfigTabKey;
  mark: string;
  name: string;
  description: string;
  builtIn: boolean;
  enabled: boolean;
  tone: ServiceConfigTone;
}

export interface ServiceConfigDraft {
  name: string;
  description: string;
}
