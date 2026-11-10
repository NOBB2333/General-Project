import { reactive } from 'vue';

export type HotkeyId = 'home' | 'projects' | 'settings' | 'sidebar' | 'notifications' | 'search' | 'help';

export interface HotkeyItem {
  id: HotkeyId;
  name: string;
  description: string;
  shortcut: string;
  enabled: boolean;
}

export interface HotkeyGroup {
  title: string;
  description: string;
  items: HotkeyItem[];
}

export const defaultHotkeyGroups: HotkeyGroup[] = [
  {
    title: '导航快捷键',
    description: '快速跳转到常用页面',
    items: [
      { id: 'home', name: '回到工作台', description: '导航到工作台首页', shortcut: '⌘ ⇧ H', enabled: true },
      { id: 'projects', name: '跳转项目中心', description: '导航到项目管理页面', shortcut: '⌘ ⇧ P', enabled: true },
      { id: 'settings', name: '跳转系统设置', description: '导航到系统设置页面', shortcut: '⌘ ⇧ S', enabled: true },
    ],
  },
  {
    title: '界面操作',
    description: '控制界面元素的显示和隐藏',
    items: [
      {
        id: 'sidebar',
        name: '收起/展开侧边栏',
        description: '切换左侧导航栏的展开/收起状态',
        shortcut: '⌘ B',
        enabled: true,
      },
      { id: 'notifications', name: '打开通知面板', description: '展开/收起通知面板', shortcut: '⌘ .', enabled: true },
    ],
  },
  {
    title: '工具快捷键',
    description: '快速访问常用工具',
    items: [
      { id: 'search', name: '唤醒搜索框', description: '聚焦顶部搜索输入框', shortcut: '⌘ K', enabled: true },
      { id: 'help', name: '显示快捷键帮助', description: '弹出快捷键列表', shortcut: '⌘ /', enabled: true },
    ],
  },
];

const storageKey = 'fisemap-hotkeys';

function cloneDefaults() {
  return structuredClone(defaultHotkeyGroups);
}

function readStored(): HotkeyGroup[] {
  try {
    const stored = window.localStorage.getItem(storageKey);
    if (!stored) return cloneDefaults();
    const savedItems = (JSON.parse(stored) as HotkeyGroup[]).flatMap((group) => group.items);
    return cloneDefaults().map((group) => ({
      ...group,
      items: group.items.map((item) => ({ ...item, ...savedItems.find((saved) => saved.id === item.id) })),
    }));
  } catch {
    return cloneDefaults();
  }
}

const hotkeyGroups = reactive<HotkeyGroup[]>(readStored());

export function formatHotkey(event: KeyboardEvent) {
  const keys: string[] = [];
  if (event.metaKey) keys.push('⌘');
  if (event.ctrlKey) keys.push('Ctrl');
  if (event.altKey) keys.push('⌥');
  if (event.shiftKey) keys.push('⇧');
  const aliases: Record<string, string> = {
    ' ': 'Space',
    Escape: 'Esc',
    ArrowUp: '↑',
    ArrowDown: '↓',
    ArrowLeft: '←',
    ArrowRight: '→',
  };
  const plainKey = aliases[event.key] ?? (event.key.length === 1 ? event.key.toUpperCase() : event.key);
  if (!['Meta', 'Control', 'Alt', 'Shift'].includes(event.key)) keys.push(plainKey);
  return keys.join(' ');
}

export function useHotkeyPreferences() {
  function saveHotkeys() {
    window.localStorage.setItem(storageKey, JSON.stringify(hotkeyGroups));
  }

  function resetHotkeys() {
    hotkeyGroups.splice(0, hotkeyGroups.length, ...cloneDefaults());
    window.localStorage.removeItem(storageKey);
  }

  function findHotkey(shortcut: string) {
    return hotkeyGroups.flatMap((group) => group.items).find((item) => item.enabled && item.shortcut === shortcut);
  }

  return { hotkeyGroups, saveHotkeys, resetHotkeys, findHotkey };
}
