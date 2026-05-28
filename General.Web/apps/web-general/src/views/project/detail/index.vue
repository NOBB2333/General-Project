<script lang="ts" setup>
import { computed, provide } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Empty, Select, Tabs, Tag } from 'ant-design-vue';

import ProjectDeliveryTab from './components/ProjectDeliveryTab.vue';
import ProjectDocsTab from './components/ProjectDocsTab.vue';
import ProjectIssuesTab from './components/ProjectIssuesTab.vue';
import ProjectOverviewTab from './components/ProjectOverviewTab.vue';
import ProjectPlanTab from './components/ProjectPlanTab.vue';
import ProjectResourcesTab from './components/ProjectResourcesTab.vue';
import ProjectTasksTab from './components/ProjectTasksTab.vue';
import { useProjectDetailPage } from './composables/useProjectDetailPage';
import { projectDetailContextKey } from './context';

defineOptions({ name: 'ProjectDetailPage' });

const page = useProjectDetailPage();

const {
  activeTab,
  bindTabContent,
  compactMetrics,
  detail,
  formatDate,
  handleProjectChange,
  loadingProjects,
  projectOptions,
  selectedProjectId,
  tabBodyStyle,
  tabItems,
} = page;

const tabComponentMap = {
  overview: ProjectOverviewTab,
  plan: ProjectPlanTab,
  tasks: ProjectTasksTab,
  issues: ProjectIssuesTab,
  delivery: ProjectDeliveryTab,
  docs: ProjectDocsTab,
  resources: ProjectResourcesTab,
};
const activeTabComponent = computed(() => tabComponentMap[activeTab.value as keyof typeof tabComponentMap] ?? ProjectOverviewTab);

provide(projectDetailContextKey, page);
</script>

<template>
  <Page description="" title="项目详情">
    <section class="project-detail">
      <Card :bordered="false" class="project-detail__compact-header">
        <div class="project-detail__compact-main">
          <div class="project-detail__compact-copy">
            <div class="project-detail__meta-line">
              <Tag color="processing">项目详情</Tag>
              <Tag v-if="detail?.isKeyProject" color="gold">重点项目</Tag>
              <Tag color="blue">{{ detail?.myRelation || '请选择项目' }}</Tag>
            </div>
            <h2>{{ detail?.name || '选择一个项目开始查看' }}</h2>
            <div class="project-detail__meta-pills">
              <span>{{ detail?.projectCode || '-' }}</span>
              <span>{{ detail?.organizationUnitName || '-' }}</span>
              <span>{{ detail?.managerName || '-' }}</span>
              <span>{{ formatDate(detail?.plannedStartDate) }} - {{ formatDate(detail?.plannedEndDate) }}</span>
            </div>
          </div>
          <Select
            v-model:value="selectedProjectId"
            class="project-detail__selector"
            :loading="loadingProjects"
            :options="projectOptions.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id }))"
            placeholder="切换项目"
            show-search
            @change="(value) => handleProjectChange(typeof value === 'string' ? value : undefined)"
          />
        </div>
        <div class="project-detail__compact-stats">
          <article v-for="metric in compactMetrics" :key="metric.label" class="project-detail__compact-stat">
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
          </article>
        </div>
        <Tabs v-if="selectedProjectId" v-model:activeKey="activeTab" class="project-detail__tabs" size="large">
          <Tabs.TabPane v-for="tab in tabItems" :key="tab.key" :tab="tab.label" />
        </Tabs>
      </Card>

      <div v-if="!selectedProjectId" class="project-detail__empty">
        <Empty description="请选择项目" />
      </div>

      <div v-else class="project-detail__tab-shell" :style="tabBodyStyle">
        <div :ref="bindTabContent">
          <component :is="activeTabComponent" />
        </div>
      </div>
    </section>
  </Page>
</template>

<style>
.project-detail {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.project-detail__compact-header {
  background:
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.project-detail__compact-main {
  display: flex;
  gap: 16px;
  align-items: center;
  justify-content: space-between;
}

.project-detail__compact-copy h2 {
  margin: 6px 0 8px;
  color: var(--ant-color-text);
  font-size: 22px;
  line-height: 1.2;
}

.project-detail__meta-line,
.project-detail__meta-pills,
.project-detail__compact-stats,
.project-detail__item-head,
.project-detail__tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.project-detail__meta-pills span {
  padding: 4px 10px;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-tertiary);
  border-radius: 999px;
  font-size: 12px;
}

.project-detail__selector {
  width: 320px;
}

.project-detail__compact-stats {
  margin-top: 12px;
}

.project-detail__compact-stat {
  min-width: 160px;
  padding: 10px 12px;
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__compact-stat span,
.project-detail__toolbar-label,
.project-detail__info-grid span,
.project-detail__timeline-side small {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.project-detail__compact-stat strong,
.project-detail__metric-value {
  display: block;
  margin-top: 4px;
  color: var(--ant-color-text);
  font-size: 18px;
}

.project-detail__tabs {
  margin-top: 16px;
}

.project-detail__tabs .ant-tabs-nav {
  margin-bottom: 0;
}

.project-detail__tabs .ant-tabs-tab {
  padding: 8px 0 14px;
}

.project-detail__tab-shell {
  overflow: visible;
  transition: min-height 0.28s ease;
}

.project-detail__empty {
  display: grid;
  min-height: 320px;
  place-items: center;
}

.project-detail__section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-detail__overview-grid {
  display: grid;
  gap: 16px;
  align-items: stretch;
}

.project-detail__overview-grid--cards {
  grid-template-columns: minmax(0, 1.45fr) minmax(0, 1fr);
}

.project-detail__overview-card {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.project-detail__overview-card .ant-card-head {
  flex-shrink: 0;
}

.project-detail__overview-card .ant-card-body {
  flex: 1;
  overflow: auto;
}

.project-detail__inner-card {
  border: 1px solid var(--ant-color-border-secondary);
}

.project-detail__inline-form-card {
  margin-top: 12px;
  background: var(--ant-color-fill-quaternary);
  border: 1px dashed var(--ant-color-border);
  border-radius: 8px;
}

.project-detail__overview-hero {
  align-items: center;
  display: grid;
  gap: 14px;
  grid-template-columns: minmax(0, 1.5fr) minmax(320px, 1fr);
  padding: 16px 18px;
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
  border-radius: 18px;
}

.project-detail__overview-hero-main {
  min-width: 0;
}

.project-detail__eyebrow {
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.project-detail__overview-hero-main h3 {
  margin: 8px 0 0;
  font-size: 24px;
}

.project-detail__overview-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 12px;
}

.project-detail__overview-meta span,
.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  background: var(--ant-color-fill-tertiary);
  border-radius: 999px;
  color: var(--ant-color-text-secondary);
  padding: 4px 10px;
}

.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  line-height: 1.7;
}

.project-detail__overview-kpis {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  min-width: 0;
}

.project-detail__overview-kpi,
.project-detail__issue-metric {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 12px 14px;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__info-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__detail-sections,
.project-detail__split-section {
  display: grid;
  gap: 14px;
}

.project-detail__detail-section,
.project-detail__inline-section,
.project-detail__summary-card {
  display: grid;
  gap: 10px;
}

.project-detail__detail-section header,
.project-detail__inline-section header,
.project-detail__summary-card span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
}

.project-detail__info-item,
.project-detail__overview-item {
  padding: 12px 14px;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__info-grid div,
.project-detail__overview-list div {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.project-detail__overview-list {
  display: grid;
  gap: 12px;
}

.project-detail__overview-list--metrics {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin-top: 14px;
}

.project-detail__overview-list--dense {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.project-detail__form,
.project-detail__inline-form,
.project-detail__dual-form {
  margin-top: 12px;
}

.project-detail__form .ant-form-item,
.project-detail__inline-form .ant-form-item,
.project-detail__dual-form .ant-form-item,
.project-detail__quality-link-grid .ant-form-item,
.project-detail__wbs-grid .ant-form-item,
.project-detail__risk-grid .ant-form-item {
  margin-bottom: 12px;
}

.project-detail__dual-form {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__dual-form form {
  min-width: 0;
  padding: 12px;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 12px;
}

.project-detail__member-role-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__risk-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: 96px 96px minmax(0, 1fr);
  margin-top: 10px;
}

.project-detail__wbs-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(100px, 0.8fr) minmax(180px, 1.2fr) 88px repeat(2, minmax(120px, 1fr));
  margin-top: 10px;
}

.project-detail__quality-link-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(2, minmax(180px, 1fr)) minmax(180px, 1fr) 120px;
  margin-top: 10px;
}

.project-detail__summary {
  margin: 16px 0 0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__summary-card {
  padding: 14px 16px;
  background: var(--ant-color-fill-tertiary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__summary-card p {
  margin: 0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__health {
  display: flex;
  gap: 18px;
  align-items: center;
}

.project-detail__health--panel {
  align-items: flex-start;
}

.project-detail__health-copy {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.project-detail__health-copy strong,
.project-detail__stack-item strong,
.project-detail__kanban-card strong,
.project-detail__timeline-side strong {
  color: var(--ant-color-text);
}

.project-detail__stack-list,
.project-detail__kanban-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.project-detail__stack-item,
.project-detail__kanban-card {
  padding: 12px 14px;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 12px;
}

.project-detail__stack-item span,
.project-detail__kanban-card span,
.project-detail__muted {
  color: var(--ant-color-text-secondary);
}

.project-detail__toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
  margin-bottom: 12px;
}

.project-detail__alert {
  margin-bottom: 12px;
}

.project-detail__kanban {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.project-detail__kanban-column {
  padding: 12px;
  background: var(--ant-color-fill-tertiary);
  border-radius: 16px;
}

.project-detail__kanban-column header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.project-detail__calendar-head,
.project-detail__calendar-weekdays,
.project-detail__calendar-item,
.project-detail__workflow-step {
  display: flex;
}

.project-detail__calendar-head {
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.project-detail__calendar-range-panel {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.project-detail__calendar-weekdays {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  margin-bottom: 8px;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-align: center;
}

.project-detail__calendar-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-weekdays--sticky {
  grid-template-columns: 180px repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-range-rows {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.project-detail__calendar-range-row {
  display: grid;
  gap: 12px;
  grid-template-columns: 180px minmax(0, 1fr);
}

.project-detail__calendar-range-label {
  padding: 18px 14px;
  font-weight: 600;
  background: var(--ant-color-fill-quaternary);
  border-radius: 14px;
}

.project-detail__calendar-range-track {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.project-detail__calendar-range-days,
.project-detail__calendar-range-bars {
  display: grid;
  gap: 8px;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-range-day {
  padding: 10px 0;
  text-align: center;
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__calendar-range-day--muted {
  opacity: 0.55;
}

.project-detail__calendar-range-day--today {
  color: var(--ant-color-primary);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.18);
}

.project-detail__calendar-range-empty {
  padding: 14px 16px;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__calendar-range-bar {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-height: 58px;
  padding: 10px 12px;
  color: white;
  border-radius: 16px;
  box-shadow: 0 10px 24px rgb(15 23 42 / 0.14);
}

.project-detail__calendar-range-bar span {
  color: rgb(255 255 255 / 0.86);
  font-size: 12px;
}

.project-detail__calendar-cell {
  min-height: 148px;
  padding: 12px;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 16px;
}

.project-detail__calendar-cell header,
.project-detail__workflow-step {
  justify-content: space-between;
}

.project-detail__calendar-cell header {
  display: flex;
  margin-bottom: 10px;
}

.project-detail__calendar-cell--muted {
  opacity: 0.58;
}

.project-detail__calendar-cell--today {
  border-color: rgb(22 119 255 / 0.36);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.16);
}

.project-detail__calendar-list,
.project-detail__workflow-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.project-detail__calendar-item {
  gap: 8px;
  align-items: center;
  color: var(--ant-color-text);
  font-size: 12px;
  line-height: 1.5;
}

.project-detail__calendar-dot {
  width: 8px;
  height: 8px;
  flex: none;
  border-radius: 999px;
}

.project-detail__workflow-step {
  gap: 14px;
  align-items: center;
  padding: 14px 16px;
  background: var(--ant-color-fill-quaternary);
  border-radius: 14px;
}

.project-detail__workflow-grid {
  display: grid;
  gap: 16px;
}

.project-detail__workflow-card {
  padding: 18px;
  background: linear-gradient(180deg, rgb(248 250 252), var(--ant-color-bg-container));
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 20px;
}

.project-detail__workflow-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.project-detail__workflow-canvas {
  overflow-x: auto;
}

.project-detail__workflow-chain {
  display: flex;
  align-items: center;
  min-width: 920px;
}

.project-detail__workflow-node {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 152px;
  padding: 18px 16px;
  background: white;
  border: 1px solid rgb(22 119 255 / 0.1);
  border-radius: 16px;
  box-shadow: 0 14px 32px rgb(15 23 42 / 0.08);
}

.project-detail__workflow-node-count {
  display: inline-grid;
  width: 34px;
  height: 34px;
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  border-radius: 10px;
  place-items: center;
}

.project-detail__workflow-arrow {
  padding: 0 12px;
  color: var(--ant-color-text-secondary);
  font-size: 24px;
}

.project-detail__issue-metrics {
  display: grid;
  gap: 12px;
  margin-bottom: 16px;
}

.project-detail__workflow-step p {
  margin: 4px 0 0;
  color: var(--ant-color-text-secondary);
}

.project-detail__workflow-badge {
  display: grid;
  width: 44px;
  height: 44px;
  flex: none;
  font-size: 16px;
  font-weight: 700;
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  border-radius: 14px;
  place-items: center;
}

.project-detail__knowledge-layout {
  display: grid;
  gap: 16px;
  grid-template-columns: 300px minmax(0, 1fr);
}

.project-detail__knowledge-main {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-detail__knowledge-groups,
.project-detail__knowledge-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.project-detail__knowledge-group,
.project-detail__knowledge-doc {
  display: flex;
  gap: 14px;
  align-items: center;
  justify-content: space-between;
  padding: 14px 0;
  background: transparent;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  border-radius: 0;
}

.project-detail__knowledge-list {
  border-top: 1px solid var(--ant-color-border-secondary);
}

.project-detail__knowledge-group span,
.project-detail__knowledge-meta {
  color: var(--ant-color-text-secondary);
}

.project-detail__knowledge-doc-main {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 6px;
}

.project-detail__knowledge-meta {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.project-detail__knowledge-meta--single-line {
  white-space: nowrap;
}

.project-detail__timeline-shell {
  overflow-x: auto;
}

.project-detail__timeline-head,
.project-detail__timeline-row {
  display: grid;
  gap: 8px;
  min-width: 980px;
  grid-template-columns: 240px minmax(0, 1fr);
}

.project-detail__timeline-head {
  margin-bottom: 8px;
}

.project-detail__timeline-side {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.project-detail__timeline-grid {
  display: grid;
  gap: 6px;
  align-items: center;
}

.project-detail__timeline-day {
  padding: 8px 0;
  color: var(--ant-color-text-secondary);
  text-align: center;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  font-size: 12px;
}

.project-detail__timeline-bar {
  padding: 8px 10px;
  color: white;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
  text-align: center;
}


@media (max-width: 1080px) {
  .project-detail__compact-main,
  .project-detail__toolbar,
  .project-detail__health {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__overview-grid--cards,
  .project-detail__selector,
  .project-detail__info-grid,
  .project-detail__kanban,
  .project-detail__calendar-grid,
  .project-detail__knowledge-layout {
    width: 100%;
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-head,
  .project-detail__workflow-step,
  .project-detail__workflow-card-head,
  .project-detail__knowledge-group,
  .project-detail__knowledge-doc,
  .project-detail__overview-hero {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__overview-hero {
    grid-template-columns: 1fr;
  }

  .project-detail__overview-kpis {
    grid-template-columns: 1fr;
  }

  .project-detail__overview-list--metrics,
  .project-detail__overview-list--dense,
  .project-detail__dual-form,
  .project-detail__member-role-grid,
  .project-detail__quality-link-grid,
  .project-detail__risk-grid,
  .project-detail__wbs-grid,
  .project-detail__split-section {
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-weekdays--sticky,
  .project-detail__calendar-range-row {
    grid-template-columns: 1fr;
  }

  .project-detail__workflow-chain {
    min-width: 100%;
    flex-direction: column;
    align-items: stretch;
  }

  .project-detail__workflow-arrow {
    text-align: center;
    transform: rotate(90deg);
  }

  .project-detail__compact-stat {
    min-width: calc(50% - 8px);
  }
}
</style>
