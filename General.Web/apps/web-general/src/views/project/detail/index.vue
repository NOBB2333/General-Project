<script lang="ts" setup>
import { computed, provide } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Empty, Select, Tabs, Tag } from 'ant-design-vue';

import FilePreviewModal from '#/components/file/FilePreviewModal.vue';

import ProjectControlTab from './components/ProjectControlTab.vue';
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
  previewingProjectFile,
  projectFilePreviewOpen,
  selectedProjectId,
  tabBodyStyle,
  tabItems,
} = page;

const tabComponentMap = {
  overview: ProjectOverviewTab,
  plan: ProjectPlanTab,
  tasks: ProjectTasksTab,
  issues: ProjectIssuesTab,
  control: ProjectControlTab,
  delivery: ProjectDeliveryTab,
  docs: ProjectDocsTab,
  resources: ProjectResourcesTab,
};
const activeTabComponent = computed(() => tabComponentMap[activeTab.value as keyof typeof tabComponentMap] ?? ProjectOverviewTab);

provide(projectDetailContextKey, page);
</script>

<template>
  <Page description="" title="项目详情">
    <section class="project-detail flex flex-col gap-3">
      <Card :bordered="false" class="project-detail__compact-header rounded-2xl">
        <div class="project-detail__compact-main flex items-center justify-between gap-4">
          <div class="project-detail__compact-copy">
            <div class="project-detail__meta-line flex flex-wrap gap-2">
              <Tag color="processing">项目详情</Tag>
              <Tag v-if="detail?.isKeyProject" color="gold">重点项目</Tag>
              <Tag color="blue">{{ detail?.myRelation || '请选择项目' }}</Tag>
            </div>
            <h2 class="mb-2 mt-1.5">{{ detail?.name || '选择一个项目开始查看' }}</h2>
            <div class="project-detail__meta-pills flex flex-wrap gap-2">
              <span>{{ detail?.projectCode || '-' }}</span>
              <span>{{ detail?.organizationUnitName || '-' }}</span>
              <span>{{ detail?.managerName || '-' }}</span>
              <span>{{ formatDate(detail?.plannedStartDate) }} - {{ formatDate(detail?.plannedEndDate) }}</span>
            </div>
          </div>
          <Select
            v-model:value="selectedProjectId"
            class="project-detail__selector w-80"
            :loading="loadingProjects"
            :options="projectOptions.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id }))"
            placeholder="切换项目"
            show-search
            @change="(value) => handleProjectChange(typeof value === 'string' ? value : undefined)"
          />
        </div>
        <div class="project-detail__compact-stats mt-3 flex flex-wrap gap-2">
          <article v-for="metric in compactMetrics" :key="metric.label" class="project-detail__compact-stat min-w-40 rounded-xl px-3 py-2.5">
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
      <FilePreviewModal v-model:open="projectFilePreviewOpen" :file="previewingProjectFile" />
    </section>
  </Page>
</template>

<style>
@reference "@vben/tailwind-config/theme";
.project-detail__compact-header {
  background:
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.project-detail__compact-copy h2 {
  color: var(--ant-color-text);
  font-size: 24px;
  line-height: 1.2;
}

.project-detail__item-head,
.project-detail__tag-list {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
}

.project-detail__meta-pills span {
  @apply px-2.5 py-1;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-tertiary);
  @apply rounded-full;
  font-size: 12px;
}

.project-detail__compact-stat {
  background: var(--ant-color-fill-quaternary);
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
  @apply mt-1;
  color: var(--ant-color-text);
  font-size: 18px;
}

.project-detail__tabs {
  @apply mt-4;
}

.project-detail__tabs .ant-tabs-nav {
  @apply mb-0;
}

.project-detail__tabs .ant-tabs-tab {
  @apply pb-3.5 pt-2;
}

.project-detail__tab-shell {
  min-width: 0;
  overflow: visible;
  transition: min-height 0.28s ease;
}

.project-detail__tab-shell > div {
  min-width: 0;
}

.project-detail__empty {
  display: grid;
  min-height: 320px;
  place-items: center;
}

.project-detail__section {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-detail__scope-note {
  align-items: center;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
  color: var(--ant-color-text-secondary);
  display: flex;
  @apply gap-2.5;
  @apply px-3 py-2.5;
}

.project-detail__scope-note strong {
  color: var(--ant-color-text);
  flex-shrink: 0;
}

.project-detail__overview-grid {
  display: grid;
  @apply gap-4;
  align-items: stretch;
}

.project-detail__overview-grid--cards {
  grid-template-columns: minmax(0, 1.45fr) minmax(0, 1fr);
}

.project-detail__plan-grid,
.project-detail__issue-layout {
  display: grid;
  @apply gap-4;
  align-items: start;
  min-width: 0;
}

.project-detail__plan-grid {
  grid-template-columns: minmax(0, 1.35fr) minmax(320px, 0.85fr);
}

.project-detail__plan-stack {
  display: flex;
  flex-direction: column;
  @apply gap-4;
  min-width: 0;
}

.project-detail__issue-layout {
  grid-template-columns: minmax(0, 1.45fr) minmax(320px, 0.75fr);
}

.project-detail__issue-main,
.project-detail__issue-side,
.project-detail__plan-card--sprints,
.project-detail__plan-card--milestones {
  min-width: 0;
}

.project-detail__issue-side {
  display: flex;
  flex-direction: column;
  @apply gap-4;
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

.project-detail__overview-card--detail {
  grid-column: 1 / -1;
}

.project-detail__inner-card {
  border: 1px solid var(--ant-color-border-secondary);
}

.project-detail__inner-card .ant-card-body {
  min-width: 0;
}

.ant-col > .project-detail__inner-card + .project-detail__inner-card {
  @apply mt-4;
}

.project-detail__inline-form-card {
  @apply mt-3;
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px dashed var(--ant-color-border);
  @apply rounded-lg;
}

.project-detail__overview-hero {
  align-items: center;
  display: grid;
  @apply gap-3.5;
  grid-template-columns: minmax(0, 1.5fr) minmax(320px, 1fr);
  @apply px-4 py-4;
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
  @apply rounded-2xl;
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
  @apply mt-2;
  font-size: 24px;
}

.project-detail__overview-meta {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
  @apply mt-3;
}

.project-detail__overview-meta span,
.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  background: var(--ant-color-fill-tertiary);
  @apply rounded-full;
  color: var(--ant-color-text-secondary);
  @apply px-2.5 py-1;
}

.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  line-height: 1.7;
}

.project-detail__overview-kpis {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  min-width: 0;
}

.project-detail__overview-kpi,
.project-detail__issue-metric {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__info-grid {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  align-items: stretch;
}

.project-detail__metric-strip {
  display: grid;
  @apply gap-3;
  min-width: 0;
}

.project-detail__metric-strip--wbs {
  grid-template-columns: repeat(3, minmax(180px, 1fr));
}

.project-detail__metric-strip--delivery {
  grid-template-columns: repeat(4, minmax(160px, 1fr));
}

.project-detail__metric-strip--communication {
  grid-template-columns: repeat(3, minmax(140px, 1fr));
  @apply mb-4;
}

.project-detail__detail-sections,
.project-detail__split-section {
  display: grid;
  @apply gap-3.5;
}

.project-detail__split-section--acceptance {
  grid-template-columns: minmax(0, 1.1fr) minmax(260px, 0.9fr);
  @apply mb-3;
}

.project-detail__section-head {
  align-items: center;
  color: var(--ant-color-text-secondary);
  display: flex;
  font-size: 13px;
  @apply gap-3;
  justify-content: space-between;
  @apply mb-3;
}

.project-detail__section-head .ant-btn {
  flex-shrink: 0;
}

.project-detail__primary-action {
  min-width: 96px;
  height: 32px;
  font-weight: 600;
}

.project-detail__section-head--actions-only {
  justify-content: flex-end;
}

.project-detail__detail-section,
.project-detail__inline-section,
.project-detail__summary-card {
  display: grid;
  @apply gap-2.5;
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
  min-width: 0;
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__info-grid div,
.project-detail__overview-list div {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.project-detail__metric-strip .project-detail__overview-item {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  min-height: 76px;
  justify-content: center;
}

.project-detail__overview-list {
  display: grid;
  @apply gap-3;
}

.project-detail__overview-list--metrics {
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  @apply mt-3.5;
}

.project-detail__overview-list--dense {
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
}

.project-detail__form,
.project-detail__inline-form,
.project-detail__dual-form {
  @apply mt-3;
}

.project-detail__form .ant-form-item,
.project-detail__inline-form .ant-form-item,
.project-detail__dual-form .ant-form-item,
.project-detail__quality-link-grid .ant-form-item,
.project-detail__wbs-grid .ant-form-item,
.project-detail__risk-grid .ant-form-item {
  @apply mb-3;
}

.project-detail__dual-form {
  display: grid;
  @apply gap-3.5;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__dual-form form {
  min-width: 0;
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__member-role-grid {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__risk-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: 96px 96px minmax(0, 1fr);
  @apply mt-2.5;
}

.project-detail__wbs-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: minmax(100px, 0.8fr) minmax(180px, 1.2fr) 88px repeat(2, minmax(120px, 1fr));
  @apply mt-2.5;
}

.project-detail__meeting-actions {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
}

.project-detail__meeting-action-row {
  display: grid;
  @apply gap-2;
  grid-template-columns:
    minmax(220px, 1.5fr)
    minmax(92px, 0.7fr)
    minmax(120px, 0.8fr)
    minmax(120px, 0.8fr)
    repeat(3, minmax(130px, 1fr))
    minmax(96px, 0.7fr)
    52px;
  align-items: center;
}

.project-detail__wbs-layout {
  display: grid;
  @apply gap-3.5;
  grid-template-columns: minmax(360px, 1fr) minmax(360px, 0.9fr);
  min-width: 0;
}

.project-detail__wbs-panel {
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__wbs-panel--embedded {
  @apply my-2;
}

.project-detail__wbs-tree,
.project-detail__wbs-detail {
  min-width: 0;
}

.project-detail__wbs-detail {
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__wbs-form-head {
  align-items: flex-start;
  @apply mb-2.5;
}

.project-detail__wbs-form-title {
  display: grid;
  min-width: 0;
  @apply gap-1;
}

.project-detail__wbs-form-title span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__wbs-form-section {
  display: grid;
  @apply gap-2;
  @apply pt-3;
  border-top: 1px solid var(--ant-color-border-secondary);
}

.project-detail__wbs-form-section:first-of-type {
  @apply pt-0;
  border-top: 0;
}

.project-detail__wbs-form-section > span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-weight: 600;
}

.project-detail__wbs-form-section + .project-detail__wbs-form-section {
  @apply mt-1;
}

.project-detail__wbs-linked {
  display: grid;
  @apply gap-2;
  @apply mt-3;
}

.project-detail__table-row--selected > td {
  background: rgb(22 119 255 / 0.06) !important;
}

.project-detail__wbs-task-card {
  @apply mt-3.5;
}

.project-detail__link-button {
  display: inline-flex;
  max-width: 100%;
  @apply gap-2;
  align-items: center;
  @apply p-0;
  color: var(--ant-color-primary);
  text-align: left;
  background: transparent;
  border: 0;
  cursor: pointer;
}

.project-detail__link-button span {
  overflow: hidden;
  color: var(--ant-color-text);
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__quality-link-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: repeat(2, minmax(180px, 1fr)) minmax(180px, 1fr) 120px;
  @apply mt-2.5;
}

.project-detail__summary {
  @apply mt-4;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__summary-card {
  @apply px-4 py-3.5;
  background: var(--ant-color-fill-tertiary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__summary-card p {
  @apply m-0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__change-impact-form {
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__change-impact-list,
.project-detail__change-approval-panel {
  display: grid;
  @apply gap-2.5;
  @apply mt-2.5;
}

.project-detail__change-impact-item {
  display: grid;
  @apply gap-1.5;
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-tertiary);
  @apply rounded-lg;
}

.project-detail__change-impact-item small {
  color: var(--ant-color-text-secondary);
}

.project-detail__health {
  display: flex;
  @apply gap-4;
  align-items: center;
}

.project-detail__health--panel {
  align-items: flex-start;
}

.project-detail__health-copy {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
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
  @apply gap-2.5;
}

.project-detail__stack-item,
.project-detail__kanban-card {
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__requirement-files {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
  align-items: center;
  @apply mt-2.5;
}

.project-detail__stack-item span,
.project-detail__kanban-card span,
.project-detail__muted {
  color: var(--ant-color-text-secondary);
}

.project-detail__card-note {
  @apply mb-3;
}

.project-detail__toolbar {
  display: flex;
  flex-wrap: wrap;
  @apply gap-3;
  align-items: center;
  justify-content: space-between;
  @apply mb-3;
}

.project-detail__alert {
  clear: both;
  display: flex;
  flex: none;
  align-items: center;
  @apply mb-4;
  @apply pt-2.5;
  @apply pb-2.5;
  width: 100%;
}

.project-detail__alert .ant-alert-content {
  flex: 1;
  min-width: 0;
}

.project-detail__alert:not(.ant-alert-with-description) .ant-alert-icon {
  display: inline-flex;
  align-self: center;
  align-items: center;
  line-height: 1;
  @apply mt-0;
}

.project-detail__alert:not(.ant-alert-with-description) .ant-alert-content {
  display: flex;
  align-items: center;
  min-height: 22px;
}

.project-detail__alert .ant-alert-message,
.project-detail__alert .ant-alert-description {
  line-height: 1.6;
}

.project-detail__demand-workspace-head {
  display: flex;
  flex-direction: column;
  @apply gap-5;
  @apply mb-5 mt-1;
}

.project-detail__demand-toolbar {
  display: flex;
  @apply gap-4;
  align-items: center;
  justify-content: space-between;
  @apply mb-0;
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__demand-toolbar-left {
  display: flex;
  align-items: center;
  min-width: 0;
}

.project-detail__demand-view-switch {
  @apply ml-auto;
}

.project-detail__worklog-drawer {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-detail__worklog-quickbar {
  display: flex;
  @apply gap-3.5;
  align-items: center;
  justify-content: space-between;
  @apply px-4 py-3.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__worklog-quickbar-metrics,
.project-detail__worklog-quickbar-actions {
  display: flex;
  @apply gap-2;
  align-items: center;
  flex-wrap: wrap;
  min-width: 0;
}

.project-detail__worklog-quickbar-metrics span {
  color: var(--ant-color-text-secondary);
  white-space: nowrap;
}

.project-detail__worklog-quickbar-metrics strong {
  color: var(--ant-color-text);
}

.project-detail__worklog-date-input {
  width: 150px;
}

.project-detail__worklog-summary-input {
  width: min(460px, 34vw);
}

.project-detail__worklog-entry-list {
  display: flex;
  flex-direction: column;
  @apply gap-3.5;
}

.project-detail__worklog-entry-row {
  display: grid;
  @apply gap-3.5;
  grid-template-columns: minmax(300px, 360px) minmax(0, 1fr);
  @apply p-3.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__worklog-demand-info,
.project-detail__worklog-row-form {
  min-width: 0;
}

.project-detail__worklog-demand-info {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
}

.project-detail__worklog-demand-info span,
.project-detail__worklog-demand-info small {
  color: var(--ant-color-text-secondary);
  line-height: 1.5;
}

.project-detail__worklog-row-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: minmax(160px, 1fr) 120px minmax(150px, 0.8fr) 110px;
}

.project-detail__worklog-block-reason {
  @apply mb-2.5;
}

.project-detail__worklog-progress-textarea {
  height: 104px;
  max-height: 160px;
  min-height: 96px;
  overflow-y: auto;
  resize: vertical;
}

.project-detail__worklog-row-actions {
  display: flex;
  justify-content: flex-end;
  @apply mt-2.5;
}

.project-detail__task-sprint-metrics {
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
}

.project-detail__task-sprint-metrics--demand {
  @apply gap-7;
  @apply m-0;
  @apply p-0;
}

.project-detail__task-sprint-metrics--sprint {
  @apply mb-5;
  @apply p-0;
}

.project-detail__task-sprint-metrics .project-detail__overview-item {
  min-height: 82px;
  justify-content: center;
}

.project-detail__demand-description-cell {
  min-width: 320px;
  max-width: 520px;
  overflow-wrap: anywhere;
  color: var(--ant-color-text-secondary);
  line-height: 1.55;
  white-space: pre-wrap;
}

.project-detail__demand-description-input {
  min-width: 0;
  width: 100%;
}

.project-detail__demand-description-input textarea,
textarea.project-detail__demand-description-input {
  min-height: 112px;
  max-height: 260px;
  resize: vertical;
}

.project-detail__demand-grid .project-detail__demand-edit-cell {
  vertical-align: top;
}

.project-detail__demand-grid .project-detail__demand-edit-cell .vxe-cell {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  overflow: visible;
  min-height: 104px;
  @apply pt-2.5;
  @apply pb-2.5;
  white-space: normal;
}

.project-detail__demand-grid .project-detail__demand-edit-cell.col--left .vxe-cell {
  justify-content: flex-start;
}

.project-detail__demand-grid .project-detail__demand-edit-cell .ant-input,
.project-detail__demand-grid .project-detail__demand-edit-cell .ant-input-number,
.project-detail__demand-grid .project-detail__demand-edit-cell .ant-picker,
.project-detail__demand-grid .project-detail__demand-edit-cell .ant-select {
  min-width: 0;
  width: 100%;
}

.project-detail__demand-grid .project-detail__demand-edit-cell .ant-select-selector,
.project-detail__demand-grid .project-detail__demand-edit-cell .ant-input-number-input-wrap {
  min-width: 0;
}

.project-detail__demand-grid .project-detail__demand-edit-cell .ant-select-selection-overflow {
  flex-wrap: wrap;
}

.project-detail__demand-grid .project-detail__demand-edit-cell .ant-space {
  flex-wrap: nowrap;
}

.project-detail__prd-cell {
  display: flex;
  min-width: 110px;
  flex-direction: column;
  @apply gap-0.5;
}

.project-detail__prd-cell .ant-btn {
  align-self: flex-start;
  height: auto;
  @apply p-0;
  line-height: 1.4;
}

.project-detail__prd-cell span {
  color: var(--ant-color-text);
  font-size: 12px;
}

.project-detail__section .ant-table-thead > tr > th {
  white-space: nowrap;
}

.project-detail__prd-cell small {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.project-detail__filter-bar {
  display: flex;
  justify-content: flex-start;
  @apply mb-5 mt-3;
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__communication-form {
  @apply mb-5 mt-4;
}

.project-detail__communication-form .ant-row {
  @apply gap-y-1.5;
}

.project-detail__communication-form .ant-form-item {
  @apply mb-3;
}

.project-detail__communication-head {
  align-items: center;
  @apply gap-4;
  @apply mb-4;
}

.project-detail__communication-head > span {
  color: var(--ant-color-text-secondary);
  line-height: 1.6;
}

.project-detail__communication-head .ant-space {
  @apply ml-auto;
}

.project-detail__form-upload-action {
  display: flex;
  align-items: end;
}

.project-detail__form-upload-action .ant-upload,
.project-detail__form-upload-action .ant-upload-wrapper {
  width: 100%;
}

.project-detail__toolbar-label {
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.project-detail__alert-stack {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
}

.project-detail__inner-card .ant-table-wrapper {
  min-width: 0;
}

.project-detail__charter-file {
  display: flex;
  @apply gap-3;
  align-items: center;
  justify-content: space-between;
  @apply px-3.5 py-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__charter-file-main {
  display: flex;
  min-width: 0;
  flex: 1;
  flex-direction: column;
  @apply gap-2;
}

.project-detail__file-name-cell {
  display: block;
  min-width: 0;
  overflow: hidden;
  color: var(--ant-color-text);
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__file-name-button {
  max-width: 100%;
  min-width: 0;
  @apply p-0;
}

.project-detail__file-name-button > span {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__approval-panel {
  display: flex;
  flex-direction: column;
  @apply gap-3;
  @apply mb-4;
}

.project-detail__approval-records {
  @apply mt-3.5;
}

.project-detail__kanban {
  display: flex;
  width: 100%;
  @apply gap-3;
  min-width: 0;
  flex-wrap: nowrap;
  overflow-x: auto;
  @apply px-0.5 pb-3 pt-1;
}

.project-detail__kanban-column {
  flex: 0 0 280px;
  min-height: 360px;
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
  transition:
    background 0.2s ease,
    border-color 0.2s ease;
}

.project-detail__kanban-column header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  @apply mb-2.5;
}

.project-detail__kanban-column:hover {
  background: rgb(22 119 255 / 0.04);
  border-color: rgb(22 119 255 / 0.18);
}

.project-detail__kanban-list {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  min-height: 120px;
}

.project-detail__kanban-card {
  @apply p-3;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  cursor: grab;
  @apply rounded-lg;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.2s ease;
}

.project-detail__kanban-card:hover {
  border-color: rgb(22 119 255 / 0.24);
  box-shadow: 0 10px 24px rgb(15 23 42 / 0.08);
  transform: translateY(-1px);
}

.project-detail__kanban-code {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-weight: 600;
}

.project-detail__kanban-card strong {
  display: -webkit-box;
  @apply mb-1.5 mt-2;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-height: 1.45;
}

.project-detail__kanban-card span {
  display: block;
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  line-height: 1.55;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__kanban-card:active {
  cursor: grabbing;
}

.project-detail__kanban-card-actions {
  display: flex;
  justify-content: flex-end;
  @apply mt-2;
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
  @apply mb-3;
}

.project-detail__calendar-range-panel {
  display: flex;
  flex-direction: column;
  @apply gap-3;
  min-width: 0;
  overflow-x: auto;
  @apply pb-1;
}

.project-detail__calendar-weekdays {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  @apply mb-2;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-align: center;
}

.project-detail__calendar-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-weekdays--sticky {
  min-width: 1180px;
  grid-template-columns: 120px repeat(7, minmax(136px, 1fr));
}

.project-detail__calendar-range-rows {
  display: flex;
  flex-direction: column;
  @apply gap-3.5;
}

.project-detail__calendar-range-row {
  display: grid;
  @apply gap-3;
  min-width: 1180px;
  grid-template-columns: 120px minmax(0, 1fr);
}

.project-detail__calendar-range-label {
  @apply p-3;
  font-weight: 600;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__calendar-range-track {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
}

.project-detail__calendar-range-days,
.project-detail__calendar-range-bars {
  display: grid;
  @apply gap-1.5;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-range-day {
  min-height: 108px;
  @apply p-2;
  text-align: left;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__calendar-range-day--muted {
  opacity: 0.55;
}

.project-detail__calendar-range-day--today {
  color: var(--ant-color-primary);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.18);
}

.project-detail__calendar-range-empty {
  @apply px-4 py-3.5;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-quaternary);
  @apply rounded-xl;
}

.project-detail__calendar-range-bar {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  min-height: 42px;
  @apply px-2.5 py-2;
  color: white;
  @apply rounded-lg;
  box-shadow: 0 8px 18px rgb(15 23 42 / 0.12);
  cursor: pointer;
  transition:
    filter 0.2s ease,
    transform 0.2s ease;
}

.project-detail__calendar-range-bar:hover {
  filter: brightness(1.04);
  transform: translateY(-1px);
}

.project-detail__calendar-range-bar strong,
.project-detail__calendar-range-bar span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__calendar-range-bar span {
  color: rgb(255 255 255 / 0.86);
  font-size: 12px;
}

.project-detail__calendar-cell {
  min-height: 148px;
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-2xl;
}

.project-detail__calendar-cell header,
.project-detail__workflow-step {
  justify-content: space-between;
}

.project-detail__calendar-cell header {
  display: flex;
  @apply mb-2.5;
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
  @apply gap-2;
}

.project-detail__calendar-item {
  @apply gap-2;
  align-items: center;
  color: var(--ant-color-text);
  font-size: 12px;
  line-height: 1.5;
}

.project-detail__calendar-dot {
  width: 8px;
  height: 8px;
  flex: none;
  @apply rounded-full;
}

.project-detail__workflow-step {
  @apply gap-3.5;
  align-items: center;
  @apply px-4 py-3.5;
  background: var(--ant-color-fill-quaternary);
  @apply rounded-xl;
}

.project-detail__workflow-grid {
  display: grid;
  @apply gap-4;
}

.project-detail__workflow-card {
  @apply p-4;
  background:
    linear-gradient(180deg, rgb(22 119 255 / 0.035), transparent),
    var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl;
}

.project-detail__workflow-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  @apply mb-4;
}

.project-detail__workflow-canvas {
  overflow-x: auto;
}

.project-detail__x6-canvas {
  width: 100%;
  min-height: 260px;
  overflow: hidden;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__workflow-chain {
  display: flex;
  align-items: center;
  min-width: 920px;
}

.project-detail__workflow-node {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  min-width: 152px;
  @apply px-4 py-4;
  background: var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.1);
  @apply rounded-xl;
  box-shadow: 0 10px 24px rgb(15 23 42 / 0.06);
}

.project-detail__workflow-node-count {
  display: inline-grid;
  width: 34px;
  height: 34px;
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  @apply rounded-lg;
  place-items: center;
}

.project-detail__workflow-arrow {
  @apply px-3;
  color: var(--ant-color-text-secondary);
  font-size: 24px;
}

.project-detail__issue-metrics {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.project-detail__workflow-step p {
  @apply mt-1;
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
  @apply rounded-xl;
  place-items: center;
}

.project-detail__knowledge-layout {
  display: grid;
  @apply gap-4;
  grid-template-columns: 300px minmax(0, 1fr);
}

.project-detail__knowledge-main {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-detail__knowledge-sidebar {
  display: flex;
  flex-direction: column;
  @apply gap-4;
  min-width: 0;
}

.project-detail__knowledge-groups,
.project-detail__knowledge-list {
  display: flex;
  flex-direction: column;
  @apply gap-3;
}

.project-detail__file-panel {
  display: flex;
  flex-direction: column;
  @apply gap-3;
}

.project-detail__file-panel p {
  @apply m-0;
  color: var(--ant-color-text-secondary);
  line-height: 1.65;
}

.project-detail__file-summary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  @apply p-3;
  background: var(--ant-color-fill-tertiary);
  @apply rounded-lg;
}

.project-detail__file-summary span,
.project-detail__file-type {
  color: var(--ant-color-text-secondary);
}

.project-detail__file-summary strong {
  color: var(--ant-color-primary);
  font-size: 20px;
}

.project-detail__file-type {
  max-width: 360px;
  @apply mt-0.5;
  overflow: hidden;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__file-table {
  @apply mt-3;
}

.project-detail__compact-file-list {
  display: flex;
  flex-direction: column;
  @apply gap-2;
}

.project-detail__compact-file-item {
  display: flex;
  width: 100%;
  @apply gap-2;
  align-items: center;
  justify-content: space-between;
  @apply px-2.5 py-2;
  color: var(--ant-color-text);
  cursor: pointer;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__compact-file-item span {
  min-width: 0;
  overflow: hidden;
  text-align: left;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-detail__compact-file-item small {
  flex: none;
  color: var(--ant-color-text-secondary);
}

.project-detail__knowledge-group,
.project-detail__knowledge-doc {
  display: flex;
  @apply gap-3.5;
  align-items: flex-start;
  justify-content: space-between;
  @apply py-3.5;
  background: transparent;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  @apply rounded-none;
}

.project-detail__knowledge-list {
  border-top: 1px solid var(--ant-color-border-secondary);
}

.project-detail__knowledge-group span,
.project-detail__knowledge-meta {
  color: var(--ant-color-text-secondary);
}

.project-detail__knowledge-group p {
  @apply mt-1;
  @apply p-0;
  color: var(--ant-color-text-secondary);
  background: transparent;
  @apply rounded-none;
  line-height: 1.65;
}

.project-detail__knowledge-group--clickable {
  position: relative;
  cursor: pointer;
  @apply py-3 pl-3.5 pr-3;
  border: 1px solid transparent;
  border-bottom-color: var(--ant-color-border-secondary);
  @apply rounded-lg;
  transition:
    background 0.2s ease,
    box-shadow 0.2s ease,
    border-color 0.2s ease,
    color 0.2s ease;
}

.project-detail__knowledge-group--clickable:hover,
.project-detail__knowledge-group--clickable.is-active {
  background: rgb(22 119 255 / 0.06);
  border-color: rgb(22 119 255 / 0.22);
}

.project-detail__knowledge-group--clickable.is-active {
  box-shadow: inset 3px 0 0 var(--ant-color-primary);
}

.project-detail__knowledge-group--clickable.is-active strong,
.project-detail__knowledge-group--clickable.is-active span {
  color: var(--ant-color-primary);
}

.project-detail__knowledge-doc p {
  @apply m-0;
  @apply p-0;
  color: var(--ant-color-text-secondary);
  background: transparent;
  @apply rounded-none;
  line-height: 1.65;
  white-space: normal;
}

.project-detail__knowledge-doc-main {
  display: flex;
  flex: 1;
  flex-direction: column;
  @apply gap-1.5;
}

.project-detail__knowledge-meta {
  display: flex;
  @apply gap-3;
  flex-wrap: wrap;
}

.project-detail__knowledge-meta span {
  @apply px-2 py-0.5;
  background: var(--ant-color-fill-tertiary);
  @apply rounded-full;
}

.project-detail__knowledge-meta--single-line {
  white-space: nowrap;
}

.project-detail__timeline-shell {
  overflow-x: auto;
  @apply p-4;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__timeline-title {
  display: flex;
  @apply gap-2;
  align-items: baseline;
  justify-content: space-between;
  @apply mb-3;
}

.project-detail__timeline-title span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.project-detail__timeline-head,
.project-detail__timeline-row {
  display: grid;
  @apply gap-3;
  min-width: 1180px;
  grid-template-columns: 280px minmax(0, 1fr);
}

.project-detail__timeline-head {
  @apply mb-2;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-weight: 600;
}

.project-detail__timeline-side {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  justify-content: center;
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-detail__timeline-grid {
  display: grid;
  @apply gap-1.5;
  align-items: center;
  min-height: 50px;
  background-image: linear-gradient(to right, var(--ant-color-border-secondary) 1px, transparent 1px);
  background-size: calc(100% / var(--timeline-columns, 12)) 100%;
}

.project-detail__timeline-day {
  @apply py-2;
  color: var(--ant-color-text-secondary);
  text-align: center;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  font-size: 12px;
}

.project-detail__timeline-bar {
  --task-bar-color: var(--ant-color-primary);
  display: flex;
  @apply gap-2;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  min-height: 34px;
  @apply px-3 py-2;
  color: white;
  background:
    linear-gradient(90deg, rgb(255 255 255 / 0.04), rgb(255 255 255 / 0.22)),
    var(--task-bar-color);
  @apply rounded-full;
  font-size: 12px;
  text-align: center;
  text-overflow: ellipsis;
  white-space: nowrap;
  box-shadow: 0 8px 18px rgb(15 23 42 / 0.12);
}

.project-detail__timeline-bar span {
  @apply px-2 py-0.5;
  color: rgb(255 255 255 / 0.92);
  background: rgb(255 255 255 / 0.18);
  @apply rounded-full;
  font-weight: 600;
}

.project-detail__timeline-bar strong {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}


@media (min-width: 761px) and (max-width: 1080px) {
  .project-detail__compact-main,
  .project-detail__demand-toolbar,
  .project-detail__toolbar,
  .project-detail__health {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__overview-grid--cards,
  .project-detail__plan-grid,
  .project-detail__issue-layout,
  .project-detail__selector,
  .project-detail__calendar-grid,
  .project-detail__knowledge-layout,
  .project-detail__wbs-layout {
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
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .project-detail__overview-list--metrics,
  .project-detail__metric-strip--wbs,
  .project-detail__metric-strip--delivery,
  .project-detail__member-role-grid,
  .project-detail__quality-link-grid,
  .project-detail__risk-grid,
  .project-detail__split-section--acceptance,
  .project-detail__split-section {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .project-detail__dual-form,
  .project-detail__wbs-grid {
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-weekdays--sticky,
  .project-detail__calendar-range-row {
    grid-template-columns: 1fr;
  }

  .project-detail__worklog-entry-row {
    grid-template-columns: 1fr;
  }

  .project-detail__worklog-row-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
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

  .project-detail__demand-view-switch {
    @apply ml-0;
  }
}

@media (max-width: 760px) {
  .project-detail__compact-main,
  .project-detail__demand-toolbar,
  .project-detail__toolbar,
  .project-detail__health,
  .project-detail__calendar-head,
  .project-detail__workflow-step,
  .project-detail__workflow-card-head,
  .project-detail__knowledge-group,
  .project-detail__knowledge-doc {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__demand-view-switch {
    @apply ml-0;
  }

  .project-detail__selector {
    width: 100%;
  }

  .project-detail__overview-grid--cards,
  .project-detail__plan-grid,
  .project-detail__issue-layout,
  .project-detail__overview-hero,
  .project-detail__overview-kpis,
  .project-detail__info-grid,
  .project-detail__overview-list--metrics,
  .project-detail__metric-strip--wbs,
  .project-detail__metric-strip--delivery,
  .project-detail__overview-list--dense,
  .project-detail__dual-form,
  .project-detail__member-role-grid,
  .project-detail__quality-link-grid,
  .project-detail__risk-grid,
  .project-detail__wbs-grid,
  .project-detail__wbs-layout,
  .project-detail__split-section--acceptance,
  .project-detail__split-section,
  .project-detail__calendar-grid,
  .project-detail__knowledge-layout,
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
    min-width: 100%;
  }

  .project-detail__worklog-entry-row,
  .project-detail__worklog-row-grid {
    grid-template-columns: 1fr;
  }

  .project-detail__worklog-summary-input,
  .project-detail__worklog-date-input {
    width: 100%;
  }
}
</style>
