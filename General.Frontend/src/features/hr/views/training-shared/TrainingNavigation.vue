<script setup lang="ts">
import {
  BarChart3,
  BookOpen,
  BookOpenCheck,
  FileQuestion,
  FolderKanban,
  GraduationCap,
  Library,
  Settings2,
  ShieldCheck,
  Trophy,
} from 'lucide-vue-next';
import { useRouter } from 'vue-router';
const props = defineProps<{ active: string }>();
const router = useRouter();
const items = [
  { label: '培训中心', icon: GraduationCap, path: '/hr/training' },
  { label: '我的学习', icon: BookOpenCheck, path: '/hr/my-learning' },
  { label: '课程库', icon: Library, path: '/hr/training' },
  { label: '培训项目', icon: FolderKanban, path: '/hr/training' },
  { label: '题库管理', icon: FileQuestion, path: '/hr/training' },
  { label: '试卷管理', icon: BookOpen, path: '/hr/training' },
  { label: '考试中心', icon: Trophy, path: '/hr/training' },
  { label: '证书中心', icon: ShieldCheck, path: '/hr/certificates' },
  { label: '数据分析', icon: BarChart3, path: '/hr/training' },
  { label: '培训配置', icon: Settings2, path: '/hr/training' },
] as const;
function navigate(item: (typeof items)[number]) {
  if (item.path === '/hr/training' && item.label !== '培训中心')
    router.push({ path: item.path, query: { section: item.label } });
  else router.push(item.path);
}
</script>
<template>
  <nav
    :class="$style.nav"
    aria-label="培训与考试模块"
  >
    <button
      v-for="item in items"
      :key="item.label"
      type="button"
      :class="{ [$style.active]: props.active === item.label }"
      @click="navigate(item)"
    >
      <component
        :is="item.icon"
        :size="13"
        :stroke-width="1.8"
      />{{ item.label }}
    </button>
  </nav>
</template>
<style module>
@reference "../../../../styles/index.css";
.nav {
  @apply mt-3 flex h-11 items-center overflow-hidden rounded border bg-white px-2;
  border-color: var(--ui-border);
}
.nav button {
  @apply flex h-9 items-center gap-1.5 border-0 bg-transparent px-3 text-[10px] font-semibold;
  white-space: nowrap;
  color: var(--ui-text-muted);
}
.nav button:hover {
  background: var(--ui-surface-hover);
}
.nav button.active {
  @apply rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
  box-shadow: inset 0 -2px var(--app-primary);
}
</style>
