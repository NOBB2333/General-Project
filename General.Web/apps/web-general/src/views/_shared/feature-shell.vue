<script lang="ts" setup>
interface FeatureMetric {
  label: string;
  value: string;
}

const props = defineProps<{
  eyebrow: string;
  highlights: string[];
  metrics: FeatureMetric[];
  summary: string;
  title: string;
}>();
</script>

<template>
  <section class="feature-shell">
    <header class="feature-shell__hero">
      <div class="feature-shell__copy">
        <p class="feature-shell__eyebrow">{{ props.eyebrow }}</p>
        <h1 class="feature-shell__title">{{ props.title }}</h1>
        <p class="feature-shell__summary">{{ props.summary }}</p>
      </div>
      <div class="feature-shell__grid">
        <article
          v-for="metric in props.metrics"
          :key="metric.label"
          class="feature-shell__metric"
        >
          <span class="feature-shell__metric-label">{{ metric.label }}</span>
          <strong class="feature-shell__metric-value">{{ metric.value }}</strong>
        </article>
      </div>
    </header>

    <section class="feature-shell__content">
      <div class="feature-shell__card">
        <h2>一期落地重点</h2>
        <ul>
          <li v-for="highlight in props.highlights" :key="highlight">
            {{ highlight }}
          </li>
        </ul>
      </div>
      <div class="feature-shell__card feature-shell__card--accent">
        <h2>当前状态</h2>
        <p>
          当前页面已接入一期导航骨架，后续将在此页面继续补业务列表、表单、详情和权限按钮。
        </p>
      </div>
    </section>
  </section>
</template>

<style scoped>
.feature-shell {
  --feature-surface: var(--ant-color-bg-container, hsl(var(--card)));
  --feature-surface-alt: var(--ant-color-bg-elevated, var(--feature-surface));
  --feature-surface-soft: var(--ant-color-fill-quaternary, hsl(var(--accent)));
  --feature-surface-accent: linear-gradient(
    135deg,
    var(--ant-color-primary-bg, var(--feature-surface-alt)) 0%,
    var(--ant-color-bg-elevated, var(--feature-surface)) 100%
  );
  --feature-border-accent: var(
    --ant-color-primary-border,
    var(--feature-border)
  );
  --feature-border: var(--ant-color-border-secondary, hsl(var(--border)));
  --feature-text: var(--ant-color-text, hsl(var(--foreground)));
  --feature-text-secondary: var(
    --ant-color-text-secondary,
    hsl(var(--muted-foreground))
  );
  --feature-shadow: var(--ant-box-shadow-secondary, 0 24px 60px rgb(15 23 42 / 8%));

  min-height: 100%;
  padding: 28px;
  background:
    radial-gradient(circle at top left, hsl(var(--primary) / 0.16), transparent 34%),
    radial-gradient(circle at top right, hsl(var(--accent) / 0.88), transparent 30%),
    linear-gradient(180deg, hsl(var(--background)) 0%, hsl(var(--background-deep)) 100%);
  color: var(--feature-text);
  transition:
    background 0.24s ease,
    color 0.24s ease;
}

.feature-shell__hero {
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 1.35fr) minmax(320px, 0.95fr);
  align-items: stretch;
}

.feature-shell__copy,
.feature-shell__metric,
.feature-shell__card {
  border: 1px solid var(--feature-border);
  border-radius: 26px;
  backdrop-filter: blur(10px);
  box-shadow: var(--feature-shadow);
  transition:
    background-color 0.24s ease,
    background 0.24s ease,
    border-color 0.24s ease,
    box-shadow 0.24s ease,
    color 0.24s ease;
}

.feature-shell__copy {
  padding: 28px;
  background: var(--feature-surface);
}

.feature-shell__eyebrow {
  margin: 0 0 10px;
  color: var(--ant-color-primary);
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.feature-shell__title {
  margin: 0;
  color: var(--feature-text);
  font-size: clamp(32px, 4vw, 48px);
  line-height: 1.05;
}

.feature-shell__summary {
  max-width: 58ch;
  margin: 16px 0 0;
  color: var(--feature-text-secondary);
  font-size: 15px;
  line-height: 1.75;
}

.feature-shell__grid {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.feature-shell__metric {
  padding: 18px;
  background: var(--feature-surface-alt);
}

.feature-shell__metric-label {
  display: block;
  margin-bottom: 12px;
  color: var(--feature-text-secondary);
  font-size: 13px;
}

.feature-shell__metric-value {
  color: var(--feature-text);
  font-size: 28px;
  font-weight: 700;
}

.feature-shell__content {
  display: grid;
  gap: 18px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin-top: 18px;
}

.feature-shell__card {
  padding: 24px;
  background: var(--feature-surface);
}

.feature-shell__card--accent {
  background: var(--feature-surface-accent);
  border-color: var(--feature-border-accent);
  color: var(--feature-text);
}

.feature-shell__card h2 {
  margin: 0 0 14px;
  font-size: 18px;
}

.feature-shell__card ul {
  padding-left: 18px;
  margin: 0;
  color: var(--feature-text-secondary);
  line-height: 1.8;
}

.feature-shell__card--accent p {
  margin: 0;
  color: var(--feature-text-secondary);
  line-height: 1.8;
}

.feature-shell__card--accent h2 {
  color: inherit;
}

@media (max-width: 960px) {
  .feature-shell {
    padding: 18px;
  }

  .feature-shell__hero,
  .feature-shell__content,
  .feature-shell__grid {
    grid-template-columns: 1fr;
  }
}
</style>
