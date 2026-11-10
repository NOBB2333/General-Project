<script setup lang="ts">
import { Check, Grid2X2, RotateCcw, Save } from 'lucide-vue-next';
import { appearanceThemes, useAppearancePreferences } from '@/components/layout/useAppearancePreferences';
import type { FontScale, RadiusStyle, SidebarDensity, ThemeId } from '@/components/layout/useAppearancePreferences';
import { UiButton, UiSwitch } from '@/components/ui';
import SettingsPanel from './SettingsPanel.vue';

const emit = defineEmits<{ notify: [message: string] }>();
const { appearance, resetAppearance, saveAppearance } = useAppearancePreferences();

const densities: { id: SidebarDensity; name: string; description: string }[] = [
  { id: 'comfortable', name: '舒适', description: '更大的间距和字号' },
  { id: 'compact', name: '紧凑', description: '更小的间距，显示更多菜单' },
];
const fontScales: { id: FontScale; name: string }[] = [
  { id: 'small', name: '小' },
  { id: 'medium', name: '中' },
  { id: 'large', name: '大' },
];
const radii: { id: RadiusStyle; name: string }[] = [
  { id: 'square', name: '直角' },
  { id: 'small', name: '小圆角' },
  { id: 'medium', name: '中圆角' },
  { id: 'large', name: '大圆角' },
];
function selectTheme(theme: ThemeId) {
  appearance.theme = theme;
}

function reset() {
  resetAppearance();
  emit('notify', '外观设置已恢复默认');
}

function save() {
  saveAppearance();
  emit('notify', '外观设置已保存并应用到全局');
}
</script>

<template>
  <SettingsPanel
    title="外观定制"
    description="自定义系统主题色、侧边栏样式和界面参数"
  >
    <section :class="$style.section">
      <h3 :class="$style.sectionTitle">主题方案</h3>
      <div :class="$style.themeGrid">
        <button
          v-for="theme in appearanceThemes"
          :key="theme.id"
          :class="[$style.themeCard, appearance.theme === theme.id && $style.selectedCard]"
          :data-theme-preview="theme.id"
          @click="selectTheme(theme.id)"
        >
          <span :class="$style.themePreview">
            <i :class="$style.previewSidebar">
              <b
                v-for="line in 3"
                :key="line"
                :class="$style.previewSidebarLine"
              />
            </i>
            <i :class="$style.previewContent">
              <b :class="$style.previewAccent" />
              <span :class="$style.previewBlocks"
                ><em
                  v-for="block in 4"
                  :key="block"
              /></span>
            </i>
          </span>
          <span :class="$style.themeMeta">
            <strong :class="$style.themeName"><i :class="$style.previewDot" />{{ theme.name }}</strong>
            <small :class="$style.themeDescription">{{ theme.description }}</small>
          </span>
          <Check
            v-if="appearance.theme === theme.id"
            :size="18"
            :stroke-width="1.8"
            :class="$style.selectedIcon"
          />
        </button>
      </div>
    </section>

    <section :class="$style.section">
      <h3 :class="$style.sectionTitle">侧边栏密度</h3>
      <div :class="$style.densityGrid">
        <button
          v-for="item in densities"
          :key="item.id"
          :class="[$style.optionCard, appearance.density === item.id && $style.selectedCard]"
          @click="appearance.density = item.id"
        >
          <Grid2X2
            :size="17"
            :stroke-width="1.8"
            :class="$style.optionIcon"
          />
          <span
            ><strong :class="$style.optionName">{{ item.name }}</strong
            ><small :class="$style.optionDescription">{{ item.description }}</small></span
          >
        </button>
      </div>
    </section>

    <section :class="$style.section">
      <h3 :class="$style.sectionTitle">字号大小</h3>
      <div :class="$style.fontGrid">
        <button
          v-for="item in fontScales"
          :key="item.id"
          :class="[$style.fontCard, appearance.fontScale === item.id && $style.selectedCard]"
          @click="appearance.fontScale = item.id"
        >
          <strong
            :class="$style.fontSample"
            :data-font-scale-preview="item.id"
            >A</strong
          >
          <span :class="$style.fontLabel">{{ item.name }}</span>
        </button>
      </div>
    </section>

    <section :class="$style.section">
      <h3 :class="$style.sectionTitle">圆角风格</h3>
      <div :class="$style.radiusGrid">
        <button
          v-for="item in radii"
          :key="item.id"
          :class="[$style.radiusCard, appearance.radius === item.id && $style.selectedCard]"
          :data-radius-preview="item.id"
          @click="appearance.radius = item.id"
        >
          <span :class="$style.radiusLabel">{{ item.name }}</span>
        </button>
      </div>
    </section>

    <section :class="$style.switchRow">
      <div>
        <h3 :class="$style.sectionTitle">过渡动画</h3>
        <p :class="$style.switchDescription">开启菜单展开、页面切换等过渡动画效果</p>
      </div>
      <UiSwitch
        v-model="appearance.motion"
        label="过渡动画"
      />
    </section>

    <section :class="$style.watermarkCard">
      <h3 :class="$style.sectionTitle">全局水印</h3>
      <p :class="$style.watermarkDescription">在全屏范围显示当前用户姓名与日期标记，降低截图外泄风险</p>
      <div :class="$style.watermarkControl">
        <span
          ><strong :class="$style.watermarkTitle">启用水印</strong
          ><small :class="$style.watermarkHint">开启后所有页面将覆盖半透明水印</small></span
        >
        <UiSwitch
          v-model="appearance.watermark"
          label="启用全局水印"
        />
      </div>
    </section>

    <template #footer>
      <UiButton
        size="sm"
        @click="reset"
      >
        <template #leading
          ><RotateCcw
            :size="14"
            :stroke-width="1.8"
        /></template>
        恢复默认
      </UiButton>
      <UiButton
        :class="$style.saveButton"
        variant="primary"
        size="sm"
        @click="save"
      >
        <template #leading
          ><Save
            :size="14"
            :stroke-width="1.8"
        /></template>
        保存设置
      </UiButton>
    </template>
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.themeCard[data-theme-preview='atlassian'] {
  --preview-primary: #0052cc;
  --preview-dark: #091e42;
}
.themeCard[data-theme-preview='ocean'] {
  --preview-primary: #0c66e4;
  --preview-dark: #0c2d4d;
}
.themeCard[data-theme-preview='emerald'] {
  --preview-primary: #00875a;
  --preview-dark: #0b2e24;
}
.themeCard[data-theme-preview='purple'] {
  --preview-primary: #6554c0;
  --preview-dark: #201547;
}
.themeCard[data-theme-preview='orange'] {
  --preview-primary: #ff8b00;
  --preview-dark: #301e0a;
}
.themeCard[data-theme-preview='cyan'] {
  --preview-primary: #00a3bf;
  --preview-dark: #082f35;
}

.previewSidebar {
  background-color: var(--preview-dark);
}
.previewSidebarLine {
  background-color: var(--preview-primary);
}
.previewAccent {
  background-color: var(--preview-primary);
}
.previewDot {
  background-color: var(--preview-primary);
}

.radiusCard[data-radius-preview='square'] {
  border-radius: 0;
}
.radiusCard[data-radius-preview='small'] {
  border-radius: 3px;
}
.radiusCard[data-radius-preview='medium'] {
  border-radius: 7px;
}
.radiusCard[data-radius-preview='large'] {
  border-radius: 12px;
}

.section {
  @apply mt-5;
}
.sectionTitle {
  @apply text-[13px] font-semibold;
}
.themeGrid {
  @apply mt-3 grid grid-cols-3 gap-3;
}
.themeCard {
  @apply relative h-[126.3px] overflow-hidden rounded border-2 text-left transition-colors;
  border-color: var(--ui-border);
}
.themeCard:not(.selectedCard):hover {
  border-color: color-mix(in srgb, var(--app-primary) 35%, white);
}
.selectedCard {
  border-color: var(--app-primary);
  background: color-mix(in srgb, var(--app-primary-soft) 30%, transparent);
}
.themePreview {
  @apply flex h-[72px];
  background: var(--ui-surface-hover);
}
.previewSidebar {
  @apply h-full w-10 p-2 not-italic;
}
.previewSidebarLine {
  @apply mb-1 block h-1 rounded-full opacity-50;
}
.previewContent {
  @apply flex-1 p-2 not-italic;
}
.previewAccent {
  @apply mb-2 block h-2 w-14 rounded-full;
}
.previewBlocks {
  @apply grid grid-cols-2 gap-1;
}
.previewBlocks em {
  @apply h-4 rounded border bg-white;
  border-color: var(--ui-border-subtle);
}
.themeMeta {
  @apply block px-3 py-2;
}
.themeName {
  @apply flex items-center gap-2 text-[12px];
}
.previewDot {
  @apply h-3.5 w-3.5 rounded-full;
}
.themeDescription {
  @apply mt-0.5 block text-[10px];
  color: var(--ui-text-subtle);
}
.selectedIcon {
  @apply absolute right-2 top-2 rounded-full p-0.5 text-white;
  background: var(--app-primary);
}
.densityGrid {
  @apply mt-3 grid grid-cols-2 gap-3;
}
.optionCard {
  @apply flex h-[71.5px] items-center border-2 px-4 text-left;
  border-color: var(--ui-border);
}
.optionIcon {
  @apply mr-3;
  color: var(--app-primary);
}
.optionName {
  @apply block text-[13px];
}
.optionDescription {
  @apply mt-1 block text-[11px];
  color: var(--ui-text-subtle);
}
.fontGrid {
  @apply mt-3 grid grid-cols-3 gap-3;
}
.fontCard {
  @apply flex h-[69px] flex-col items-center justify-center border-2;
  border-color: var(--ui-border);
}
.fontSample[data-font-scale-preview='small'] {
  @apply text-[13px];
}
.fontSample[data-font-scale-preview='medium'] {
  @apply text-[16px];
}
.fontSample[data-font-scale-preview='large'] {
  @apply text-[19px];
}
.fontLabel {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.radiusGrid {
  @apply mt-3 grid grid-cols-4 gap-3;
}
.radiusCard {
  @apply flex h-[67.8px] items-center justify-center border-2;
  border-color: var(--ui-border);
}
.radiusLabel {
  @apply text-[12px] font-medium;
}
.switchRow {
  @apply mt-5 flex items-center justify-between border-y py-4;
  border-color: var(--ui-border-subtle);
}
.switchDescription {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.watermarkCard {
  @apply mt-5 rounded-lg border p-4;
  border-color: var(--ui-border);
}
.watermarkDescription {
  @apply mt-1 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.watermarkControl {
  @apply mt-3 flex items-center justify-between border-t pt-3;
  border-color: var(--ui-border-subtle);
}
.watermarkTitle {
  @apply block text-[12px];
}
.watermarkHint {
  @apply text-[10px];
  color: #97a0af;
}
.saveButton {
  @apply ml-auto;
}
</style>
