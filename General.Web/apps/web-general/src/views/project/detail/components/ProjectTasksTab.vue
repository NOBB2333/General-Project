<script lang="ts" setup>
import { Alert, Button, Card, Space, Table, Tag } from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  filteredTasks,
  formatDate,
  kanbanColumns,
  loading,
  taskCalendarAnchorDate,
  taskCalendarRows,
  taskCalendarTitle,
  taskColumns,
  taskScope,
  taskScopeOptions,
  taskTimelineDays,
  taskTimelineGridStyle,
  taskTimelineRows,
  taskView,
  taskViewOptions,
  shiftTaskCalendarMonth,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <Card :bordered="false" class="project-detail__inner-card">
    <div class="project-detail__toolbar">
      <Space wrap>
        <span class="project-detail__toolbar-label">范围</span>
        <Button
          v-for="option in taskScopeOptions"
          :key="option.key"
          :type="taskScope === option.key ? 'primary' : 'default'"
          @click="taskScope = option.key"
        >
          {{ option.label }}
        </Button>
      </Space>
      <Space wrap>
        <span class="project-detail__toolbar-label">视图</span>
        <Button
          v-for="option in taskViewOptions"
          :key="option.key"
          :type="taskView === option.key ? 'primary' : 'default'"
          @click="taskView = option.key"
        >
          {{ option.label }}
        </Button>
      </Space>
    </div>

    <Alert
      class="project-detail__alert"
      message="“与我相关 / 我负责 / 我参与”已收敛为统一入口；日历视图改为日历区间条，适合查看跨日任务与时间条排布。"
      show-icon
      type="info"
    />

    <template v-if="taskView === 'list'">
      <Table
        :columns="taskColumns"
        :data-source="filteredTasks"
        :loading="loading"
        :pagination="false"
        :scroll="{ x: 1200 }"
        row-key="id"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'plannedEndTime'">
            {{ formatDate((record as ProjectApi.TaskItem).plannedEndTime) }}
          </template>
          <template v-else-if="['contractClause', 'productOwnerName', 'developerOwnerName', 'testerOwnerName'].includes(String(column.key))">
            {{ (record as Record<string, string | undefined | null>)[String(column.key)] || '-' }}
          </template>
          <template v-else-if="column.key === 'status'">
            <Tag :color="(record as ProjectApi.TaskItem).isBlocked ? 'error' : 'blue'">
              {{ (record as ProjectApi.TaskItem).status }}
            </Tag>
          </template>
        </template>
      </Table>
    </template>

    <template v-else-if="taskView === 'board'">
      <div class="project-detail__kanban">
        <article v-for="column in kanbanColumns" :key="column.status" class="project-detail__kanban-column">
          <header>
            <strong>{{ column.status }}</strong>
            <span>{{ column.items.length }}</span>
          </header>
          <div v-if="column.items.length === 0" class="project-detail__muted">暂无事项</div>
          <div v-else class="project-detail__kanban-list">
            <section v-for="item in column.items" :key="item.id" class="project-detail__kanban-card">
              <div class="project-detail__item-head">
                <Tag :color="item.priority === '高' ? 'error' : 'processing'">{{ item.priority }}</Tag>
                <Tag v-if="item.isBlocked" color="error">阻塞</Tag>
              </div>
              <strong>{{ item.title }}</strong>
              <span>{{ item.ownerName }} / {{ formatDate(item.plannedEndTime) }}</span>
            </section>
          </div>
        </article>
      </div>
    </template>

    <template v-else-if="taskView === 'calendar'">
      <div class="project-detail__calendar-range-panel">
        <div class="project-detail__calendar-head">
          <div>
            <strong>{{ taskCalendarTitle }}</strong>
            <p class="project-detail__calendar-subtitle">按周展示跨日事件条，适合看任务在自然日历中的连续占用区间。</p>
          </div>
          <Space>
            <Button size="small" @click="shiftTaskCalendarMonth(-1)">上月</Button>
            <Button size="small" @click="taskCalendarAnchorDate = null">回到任务月份</Button>
            <Button size="small" @click="shiftTaskCalendarMonth(1)">下月</Button>
          </Space>
        </div>
        <div class="project-detail__calendar-weekdays project-detail__calendar-weekdays--sticky">
          <span>周区间</span>
          <span v-for="label in ['日', '一', '二', '三', '四', '五', '六']" :key="label">{{ label }}</span>
        </div>
        <div class="project-detail__calendar-range-rows">
          <article v-for="row in taskCalendarRows" :key="row.key" class="project-detail__calendar-range-row">
            <div class="project-detail__calendar-range-label">{{ row.label }}</div>
            <div class="project-detail__calendar-range-track">
              <div class="project-detail__calendar-range-days">
                <div
                  v-for="day in row.days"
                  :key="day.key"
                  :class="[
                    'project-detail__calendar-range-day',
                    {
                      'project-detail__calendar-range-day--muted': !day.inCurrentMonth,
                      'project-detail__calendar-range-day--today': day.isToday,
                    },
                  ]"
                >
                  {{ day.dayLabel }}
                </div>
              </div>
              <div v-if="row.bars.length === 0" class="project-detail__calendar-range-empty">本周暂无跨日任务</div>
              <div v-else class="project-detail__calendar-range-bars">
                <div
                  v-for="bar in row.bars"
                  :key="bar.key"
                  class="project-detail__calendar-range-bar"
                  :style="{
                    background: bar.color,
                    gridColumn: `${bar.startColumn + 1} / span ${bar.span}`,
                  }"
                >
                  <strong>{{ bar.title }}</strong>
                  <span>{{ bar.subtitle }}</span>
                </div>
              </div>
            </div>
          </article>
        </div>
      </div>
    </template>

    <template v-else>
      <div v-if="taskTimelineDays.length === 0" class="project-detail__muted">
        当前筛选下暂无可展示排期。
      </div>
      <div v-else class="project-detail__timeline-shell">
        <div class="project-detail__timeline-head">
          <div class="project-detail__timeline-side">事项</div>
          <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
            <div v-for="day in taskTimelineDays" :key="day.toISOString()" class="project-detail__timeline-day">
              {{ day.getMonth() + 1 }}/{{ day.getDate() }}
            </div>
          </div>
        </div>
        <div v-for="item in taskTimelineRows" :key="item.id" class="project-detail__timeline-row">
          <div class="project-detail__timeline-side">
            <strong>{{ item.title }}</strong>
            <small>{{ item.ownerName }} / {{ item.status }}</small>
          </div>
          <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
            <div
              class="project-detail__timeline-bar"
              :style="{
                background: item.color,
                gridColumn: `${item.startOffset + 1} / span ${item.span}`,
              }"
            >
              {{ `${formatDate(item.plannedStartTime)} - ${formatDate(item.plannedEndTime)}` }}
            </div>
          </div>
        </div>
      </div>
    </template>
  </Card>
</section>
</template>
