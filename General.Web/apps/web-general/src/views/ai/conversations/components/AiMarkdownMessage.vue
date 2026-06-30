<script lang="ts" setup>
import { computed } from 'vue';

import MarkdownRender from 'markstream-vue';
import 'markstream-vue/index.css';

defineOptions({ name: 'AiMarkdownMessage' });

const props = defineProps<{
  content?: string;
  final?: boolean;
}>();

interface ArtifactEnvelope {
  artifactCode?: string;
  payload?: Record<string, any>;
  rendererType?: string;
}

const artifact = computed(() => parseArtifact(props.content || ''));

const artifactRenderer = computed(() => (artifact.value?.rendererType || artifact.value?.artifactCode || '').toLowerCase());
const artifactPayload = computed(() => artifact.value?.payload || {});
const actionItems = computed(() => Array.isArray(artifactPayload.value.items) ? artifactPayload.value.items : []);
const risks = computed(() => Array.isArray(artifactPayload.value.risks) ? artifactPayload.value.risks : []);

function parseArtifact(content: string): ArtifactEnvelope | undefined {
  const candidates = [content.trim(), ...extractJsonCodeBlocks(content)];
  for (const candidate of candidates) {
    if (!candidate.startsWith('{')) {
      continue;
    }

    try {
      const parsed = JSON.parse(candidate) as ArtifactEnvelope;
      if ((parsed.rendererType || parsed.artifactCode) && parsed.payload && typeof parsed.payload === 'object') {
        return parsed;
      }
    } catch {
      continue;
    }
  }

  return undefined;
}

function extractJsonCodeBlocks(content: string) {
  const blocks: string[] = [];
  const pattern = /```(?:json|ai-artifact)?\s*([\s\S]*?)```/gi;
  let match = pattern.exec(content);
  while (match) {
    blocks.push((match[1] || '').trim());
    match = pattern.exec(content);
  }

  return blocks;
}
</script>

<template>
  <div v-if="artifact" class="ai-artifact-message">
    <div v-if="artifactRenderer === 'metric-card'" class="ai-artifact-message__metric">
      <span class="ai-artifact-message__label">{{ artifactPayload.title || '指标' }}</span>
      <strong>{{ artifactPayload.value || '-' }}</strong>
      <span v-if="artifactPayload.trend" class="ai-artifact-message__muted">{{ artifactPayload.trend }}</span>
      <span v-if="artifactPayload.status" class="ai-artifact-message__status">{{ artifactPayload.status }}</span>
    </div>

    <div v-else-if="artifactRenderer === 'action-list'" class="ai-artifact-message__panel">
      <div class="ai-artifact-message__title">{{ artifactPayload.title || '行动队列' }}</div>
      <div v-for="(item, index) in actionItems" :key="index" class="ai-artifact-message__action">
        <strong>{{ item.title || '未命名行动' }}</strong>
        <span>{{ item.owner || '未指定责任人' }}</span>
        <span>{{ item.dueDate || '未指定截止时间' }}</span>
        <span class="ai-artifact-message__status">{{ item.priority || '普通' }}</span>
      </div>
      <div v-if="actionItems.length === 0" class="ai-artifact-message__muted">暂无行动项</div>
    </div>

    <div v-else-if="artifactRenderer === 'risk-matrix'" class="ai-artifact-message__panel">
      <div class="ai-artifact-message__title">{{ artifactPayload.title || '风险矩阵' }}</div>
      <div class="ai-artifact-message__risk-grid">
        <div v-for="(risk, index) in risks" :key="index" class="ai-artifact-message__risk">
          <strong>{{ risk.name || '未命名风险' }}</strong>
          <span>概率 {{ risk.probability ?? '-' }}</span>
          <span>影响 {{ risk.impact ?? '-' }}</span>
          <span class="ai-artifact-message__status">{{ risk.level || '未评级' }}</span>
        </div>
      </div>
      <div v-if="risks.length === 0" class="ai-artifact-message__muted">暂无风险项</div>
    </div>

    <div v-else class="ai-markdown-message">
      <MarkdownRender
        :content="props.content || ''"
        :fade="false"
        :final="props.final ?? true"
        html-policy="safe"
        mode="chat"
        render-code-blocks-as-pre
        smooth-streaming="auto"
      />
    </div>
  </div>
  <div v-else class="ai-markdown-message">
    <MarkdownRender
      :content="props.content || ''"
      :fade="false"
      :final="props.final ?? true"
      html-policy="safe"
      mode="chat"
      render-code-blocks-as-pre
      smooth-streaming="auto"
    />
  </div>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-markdown-message {
  color: var(--ant-color-text);
  font-size: 14px;
  line-height: 1.75;
  overflow-wrap: anywhere;
}

.ai-markdown-message :deep(p) {
  @apply mb-2;
}

.ai-markdown-message :deep(p:last-child),
.ai-markdown-message :deep(ul:last-child),
.ai-markdown-message :deep(ol:last-child),
.ai-markdown-message :deep(pre:last-child),
.ai-markdown-message :deep(blockquote:last-child),
.ai-markdown-message :deep(table:last-child) {
  @apply mb-0;
}

.ai-markdown-message :deep(ul),
.ai-markdown-message :deep(ol) {
  @apply pl-5;
  @apply my-2;
}

.ai-markdown-message :deep(blockquote) {
  @apply px-3 py-2;
  @apply my-2;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-quaternary);
  border-left: 3px solid var(--ant-color-primary-border);
}

.ai-markdown-message :deep(code) {
  @apply px-1 py-0.5;
  font-size: 12px;
  background: var(--ant-color-fill-secondary);
  @apply rounded;
}

.ai-markdown-message :deep(pre) {
  @apply p-3;
  @apply my-2.5;
  overflow: auto;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-markdown-message :deep(pre code) {
  @apply p-0;
  background: transparent;
}

.ai-markdown-message :deep(table) {
  width: 100%;
  @apply my-2.5;
  border-collapse: collapse;
}

.ai-markdown-message :deep(th),
.ai-markdown-message :deep(td) {
  @apply px-2.5 py-2;
  border: 1px solid var(--ant-color-border-secondary);
}

.ai-markdown-message :deep(th) {
  background: var(--ant-color-fill-quaternary);
}

.ai-artifact-message {
  width: min(100%, 720px);
}

.ai-artifact-message__metric,
.ai-artifact-message__panel {
  @apply p-3;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-artifact-message__metric {
  display: grid;
  grid-template-columns: minmax(120px, 1fr) auto;
  @apply gap-x-3 gap-y-1;
  align-items: center;
}

.ai-artifact-message__metric strong {
  font-size: 24px;
  line-height: 1.2;
}

.ai-artifact-message__label,
.ai-artifact-message__title {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.ai-artifact-message__title {
  @apply mb-2.5;
  font-weight: 600;
}

.ai-artifact-message__muted {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
}

.ai-artifact-message__status {
  justify-self: end;
  @apply px-2 py-px;
  color: var(--ant-color-primary);
  background: var(--ant-color-primary-bg);
  @apply rounded-full;
}

.ai-artifact-message__action,
.ai-artifact-message__risk {
  display: grid;
  grid-template-columns: minmax(160px, 1fr) 120px 140px auto;
  @apply gap-2;
  align-items: center;
  @apply py-2;
  border-top: 1px solid var(--ant-color-border-secondary);
}

.ai-artifact-message__action:first-of-type,
.ai-artifact-message__risk:first-of-type {
  border-top: 0;
}

.ai-artifact-message__risk-grid {
  display: grid;
  @apply gap-1;
}
</style>
