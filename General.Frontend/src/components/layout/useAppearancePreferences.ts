import { reactive, watchEffect } from 'vue';

export type ThemeId = 'atlassian' | 'ocean' | 'emerald' | 'purple' | 'orange' | 'cyan';
export type SidebarDensity = 'comfortable' | 'compact';
export type FontScale = 'small' | 'medium' | 'large';
export type RadiusStyle = 'square' | 'small' | 'medium' | 'large';

export interface AppearanceState {
  theme: ThemeId;
  density: SidebarDensity;
  fontScale: FontScale;
  radius: RadiusStyle;
  motion: boolean;
  watermark: boolean;
}

export const appearanceThemes = [
  {
    id: 'atlassian' as const,
    name: 'Atlassian Blue',
    description: '默认主题，稳重的企业蓝',
    primary: '#0052CC',
    soft: '#DEEBFF',
    dark: '#091E42',
  },
  {
    id: 'ocean' as const,
    name: '海洋蓝',
    description: '更鲜亮的蓝色调',
    primary: '#0C66E4',
    soft: '#E9F2FF',
    dark: '#0C2D4D',
  },
  {
    id: 'emerald' as const,
    name: '翡翠绿',
    description: '清新自然的绿色系',
    primary: '#00875A',
    soft: '#E3FCEF',
    dark: '#0B2E24',
  },
  {
    id: 'purple' as const,
    name: '高贵紫',
    description: '优雅的紫色主题',
    primary: '#6554C0',
    soft: '#EAE6FF',
    dark: '#201547',
  },
  {
    id: 'orange' as const,
    name: '活力橙',
    description: '充满活力的暖色调',
    primary: '#FF8B00',
    soft: '#FFFAE6',
    dark: '#301E0A',
  },
  {
    id: 'cyan' as const,
    name: '青碧',
    description: '清新现代的青色',
    primary: '#00A3BF',
    soft: '#E6FCFF',
    dark: '#082F35',
  },
];

const defaults: AppearanceState = {
  theme: 'atlassian',
  density: 'comfortable',
  fontScale: 'medium',
  radius: 'medium',
  motion: true,
  watermark: false,
};

function readStored(): AppearanceState {
  try {
    const value = window.localStorage.getItem('fisemap-appearance');
    return value ? { ...defaults, ...(JSON.parse(value) as Partial<AppearanceState>) } : { ...defaults };
  } catch {
    return { ...defaults };
  }
}

const appearance = reactive<AppearanceState>(readStored());

watchEffect(() => {
  const root = document.documentElement;
  root.dataset.appTheme = appearance.theme;
  root.dataset.density = appearance.density;
  root.dataset.radius = appearance.radius;
  root.dataset.motion = appearance.motion ? 'on' : 'off';
  root.dataset.fontScale = appearance.fontScale;
  root.dataset.themeReady = appearance.theme;
});

export function useAppearancePreferences() {
  function saveAppearance() {
    window.localStorage.setItem('fisemap-appearance', JSON.stringify(appearance));
  }

  function resetAppearance() {
    Object.assign(appearance, defaults);
    window.localStorage.removeItem('fisemap-appearance');
  }

  return { appearance, saveAppearance, resetAppearance };
}
