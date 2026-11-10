<script setup lang="ts">
import {
  CalendarDays,
  Check,
  Columns3,
  Download,
  Filter,
  LayoutDashboard,
  MoreHorizontal,
  Package,
  Plus,
  RefreshCw,
  Search,
  Settings2,
  Trash2,
  Upload,
  Warehouse,
  X,
} from 'lucide-vue-next';
import { computed, toRef } from 'vue';
import { UiAlertDialog, UiButton, UiCheckbox, UiDialog, UiDropdownMenu, UiSelect } from '@/components/ui';
import { resolvePageFamily } from '../families';
import type { PageModel, StatTone } from '../page.types';
import { useScaffoldFamilyData } from './useScaffoldFamilyData';
import { useScaffoldPageState } from './useScaffoldPageState';

const props = defineProps<{ model: PageModel }>();
const page = toRef(props, 'model');
const state = useScaffoldPageState(page);
const PAGE_SIZE_OPTIONS = [10, 20, 50].map((value) => ({ value: String(value), label: String(value) }));

const {
  rows,
  selected,
  activeTab,
  query,
  filterValues,
  detailOpen,
  createOpen,
  confirmOpen,
  columnOpen,
  importOpen,
  pendingAction,
  activeRow,
  draft,
  sortKey,
  sortDir,
  pageIndex,
  pageSize,
  activeStatKey,
  filteredRows,
  pagedRows,
  totalPages,
  dataColumns,
  allDataColumns,
  rowActions,
  allSelected,
  selectedCount,
  notify,
  toggleRow,
  toggleAll,
  setSort,
  openDetail,
  openCreate,
  openEdit,
  openCopy,
  askDelete,
  runTool,
  createRecord,
  confirmPending,
  importDemoRows,
  toggleColumn,
  showAllColumns,
  resetColumns,
  setPage,
  setPageSize,
  clickStat,
  resetFilters,
  formatCell,
  statusTone,
  primaryFieldKeys,
} = state;

const isBoardLayout = computed(() => {
  const surface = page.value.surface;
  return surface === 'board' || surface === 'dashboard';
});

const isCardsLayout = computed(() => page.value.surface === 'cards');
const isKanbanLayout = computed(() => page.value.surface === 'kanban');
const isLoginCardsLayout = computed(() => page.value.surface === 'login-cards');
const isTargetBoardLayout = computed(() => page.value.surface === 'target-board');
const isScheduleLayout = computed(() => page.value.surface === 'schedule');
const isMonthCalendarLayout = computed(() => page.value.surface === 'month-calendar');
const isCalendarLayout = computed(() => page.value.surface === 'calendar');

const isListSurface = computed(() => {
  const surface = page.value.surface;
  return (
    surface === 'list' ||
    surface === 'checklist' ||
    surface === 'settings' ||
    surface === 'workbench' ||
    surface === 'onboarding'
  );
});

const moreToolItems = computed(() => {
  const tools = [...page.value.tools.slice(4), ...(page.value.moreTools ?? [])];
  return [
    ...tools.map((tool) => ({
      id: tool,
      label: tool,
      tone: /删除|清空|停用/.test(tool) ? ('danger' as const) : ('default' as const),
    })),
    { id: 'scaffold.columns', label: '列设置', separatorBefore: tools.length > 0 },
    { id: 'scaffold.reset', label: '重置筛选' },
  ];
});

const {
  boardPanels,
  kanbanLanes,
  loginGroups,
  scheduleUnscheduled,
  schedulePeople,
  scheduleDays,
  monthMeta,
  monthCells,
  eventsForScheduleDay,
  targetTeamRows,
  targetPersonRows,
} = useScaffoldFamilyData(page, filteredRows);

function trafficTone(value: string) {
  if (/红|逾期|高|危险|停用/.test(value)) return 'danger';
  if (/黄|待|审批|中/.test(value)) return 'warning';
  return 'success';
}

function toneClass(tone: StatTone) {
  return {
    primary: 'statPrimary',
    success: 'statSuccess',
    warning: 'statWarning',
    danger: 'statDanger',
    neutral: 'statNeutral',
  }[tone];
}

function progressLevel(progress: string) {
  const num = Number.parseFloat(progress.replace('%', ''));
  if (Number.isNaN(num) || num <= 0) return 'progress0';
  if (num < 15) return 'progress10';
  if (num < 30) return 'progress25';
  if (num < 50) return 'progress40';
  if (num < 70) return 'progress60';
  if (num < 90) return 'progress80';
  return 'progress100';
}

function toolIcon(tool: string) {
  if (tool.includes('新建') || tool.includes('新增') || tool.includes('发起')) return Plus;
  if (tool.includes('导入') || tool.includes('扫描')) return Upload;
  if (tool.includes('导出') || tool.includes('打印')) return Download;
  if (tool.includes('刷新') || tool.includes('同步')) return RefreshCw;
  if (tool.includes('设置') || tool.includes('自定义') || tool.includes('配置')) return Settings2;
  if (tool.includes('删除') || tool.includes('回收')) return Trash2;
  return MoreHorizontal;
}

function filterOptions(options: string[]) {
  return options.map((option) => ({ value: option, label: option }));
}

function runMoreTool(id: string) {
  if (id === 'scaffold.reset') {
    resetFilters();
    return;
  }
  if (id === 'scaffold.columns') {
    runTool('列设置');
    return;
  }
  runTool(id);
}
</script>

<template>
  <div
    v-if="true"
    :class="$style.page"
    data-domain-page
    :data-page-label="page.label"
    :data-page-module="page.module"
    :data-page-surface="page.surface"
    :data-page-family="resolvePageFamily(page.surface)"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <div :class="$style.breadcrumb">
          <span>{{ page.module }}</span>
          <span :class="$style.breadcrumbSep">/</span>
          <span>{{ page.label }}</span>
        </div>
        <h1 :class="$style.title">{{ page.label }}</h1>
        <p :class="$style.description">{{ page.description }}</p>
      </div>
      <div :class="$style.heroActions">
        <UiButton
          v-for="tool in page.tools.slice(0, 4)"
          :key="tool"
          :variant="
            tool === page.primaryAction || tool.includes('新建') || tool.includes('新增') ? 'primary' : 'secondary'
          "
          size="sm"
          @click="runTool(tool)"
        >
          <template #leading>
            <component
              :is="toolIcon(tool)"
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          {{ tool }}
        </UiButton>
        <UiDropdownMenu
          v-if="page.moreTools?.length || page.tools.length > 4"
          :items="moreToolItems"
          label="更多操作"
          @select="runMoreTool"
        >
          <template #trigger>
            <UiButton
              variant="secondary"
              size="sm"
            >
              <template #leading>
                <MoreHorizontal
                  :size="14"
                  :stroke-width="1.8"
                />
              </template>
              更多
            </UiButton>
          </template>
        </UiDropdownMenu>
      </div>
    </header>

    <section
      v-if="page.stats.length"
      :class="$style.stats"
      data-module-stats
    >
      <button
        v-for="stat in page.stats"
        :key="stat.key"
        type="button"
        :class="[$style.statCard, $style[toneClass(stat.tone)], activeStatKey === stat.key && $style.statCardActive]"
        @click="clickStat(stat.key)"
      >
        <span :class="$style.statLabel">{{ stat.label }}</span>
        <strong :class="$style.statValue">{{ stat.value }}</strong>
      </button>
    </section>

    <section
      v-if="isBoardLayout"
      :class="$style.dashboardGrid"
      data-module-dashboard
      data-module-board
    >
      <article
        v-for="panel in boardPanels"
        :key="panel.key"
        :class="$style.dashboardPanel"
      >
        <header :class="$style.panelHeader">
          <LayoutDashboard
            :size="15"
            :stroke-width="1.8"
          />
          <h2>{{ panel.title }}</h2>
        </header>
        <ul :class="$style.panelList">
          <li
            v-for="(item, index) in panel.items"
            :key="`${panel.key}-${index}`"
          >
            <button
              type="button"
              :class="$style.panelItem"
              @click="notify(`已打开：${item}`)"
            >
              <span>{{ item }}</span>
              <small>查看</small>
            </button>
          </li>
        </ul>
      </article>
    </section>

    <section
      v-if="isBoardLayout && filteredRows.length"
      :class="$style.boardFeed"
      data-module-board-feed
    >
      <header :class="$style.panelHeader">
        <LayoutDashboard
          :size="15"
          :stroke-width="1.8"
        />
        <h2>{{ page.label }}明细</h2>
      </header>
      <div :class="$style.boardFeedList">
        <button
          v-for="row in filteredRows"
          :key="String(row.id)"
          type="button"
          :class="$style.boardFeedItem"
          @click="openDetail(row)"
        >
          <div>
            <strong>{{ row.name || row.title }}</strong>
            <span>{{ row.code }} · {{ row.type || row.status }}</span>
          </div>
          <small>{{ row.value || row.metric || row.progress || '—' }}</small>
        </button>
      </div>
    </section>

    <section
      v-if="isCardsLayout"
      :class="$style.cardGrid"
      data-module-cards
    >
      <article
        v-for="row in filteredRows"
        :key="String(row.id)"
        :class="$style.entityCard"
      >
        <header :class="$style.entityCardHeader">
          <component
            :is="page.label.includes('仓库') ? Warehouse : Package"
            :size="16"
            :stroke-width="1.8"
          />
          <div>
            <h2>{{ row.name || row.title }}</h2>
            <p>{{ row.code }} · {{ row.type || row.supplier || row.party || '—' }}</p>
          </div>
          <span
            v-if="row.status"
            :class="[$style.status, $style[statusTone(String(row.status))]]"
            >{{ row.status }}</span
          >
        </header>
        <dl :class="$style.entityCardMeta">
          <div>
            <dt>{{ page.label.includes('价格') ? '物料数' : '金额/指标' }}</dt>
            <dd>{{ row.qty || row.metric || formatCell(row, 'amount') }}</dd>
          </div>
          <div>
            <dt>{{ page.label.includes('价格') ? '供应商' : 'SKU/数值' }}</dt>
            <dd>{{ row.supplier || row.value || row.party || '—' }}</dd>
          </div>
          <div>
            <dt>负责人</dt>
            <dd>{{ row.owner || '—' }}</dd>
          </div>
          <div>
            <dt>更新</dt>
            <dd>{{ row.updatedAt || row.date || '—' }}</dd>
          </div>
        </dl>
        <div :class="$style.entityCardActions">
          <button
            type="button"
            :class="$style.linkButton"
            @click="openDetail(row)"
          >
            查看
          </button>
          <button
            type="button"
            :class="$style.linkButton"
            @click="openEdit(row)"
          >
            编辑
          </button>
        </div>
      </article>
    </section>

    <section
      v-if="isCalendarLayout"
      :class="$style.calendarBoard"
      data-module-calendar
    >
      <header :class="$style.panelHeader">
        <CalendarDays
          :size="15"
          :stroke-width="1.8"
        />
        <h2>{{ page.label }}日程</h2>
      </header>
      <div :class="$style.calendarGrid">
        <button
          v-for="row in filteredRows"
          :key="String(row.id)"
          type="button"
          :class="$style.calendarCard"
          @click="openDetail(row)"
        >
          <strong>{{ row.title || row.name }}</strong>
          <span>{{ row.date }} {{ row.time }}</span>
          <small>{{ row.owner }} · {{ row.status }}</small>
        </button>
      </div>
    </section>

    <section
      v-if="isLoginCardsLayout"
      :class="$style.loginSurface"
      data-module-login-cards
    >
      <div :class="$style.loginHero">
        <h2>请选择工号登录</h2>
        <p>车间报工 · 触屏模式 · 选择工号登录后接收派工任务</p>
      </div>
      <div
        v-for="group in loginGroups"
        :key="group.key"
        :class="$style.loginGroup"
      >
        <header :class="$style.panelHeader">
          <h2>{{ group.title }}</h2>
          <small>{{ group.rows.length }} 人</small>
        </header>
        <div :class="$style.loginGrid">
          <button
            v-for="row in group.rows"
            :key="String(row.id)"
            type="button"
            :class="$style.loginCard"
            @click="notify(`已选择工号 ${row.code} · ${row.name}`)"
          >
            <span :class="$style.loginCode">{{ row.code }}</span>
            <strong>{{ row.name }}</strong>
            <small>{{ row.dept || row.type || row.metric }}</small>
            <em>{{ row.status || '可登录' }}</em>
          </button>
        </div>
      </div>
    </section>

    <section
      v-if="isKanbanLayout"
      :class="$style.kanbanBoard"
      data-module-kanban
    >
      <div
        v-for="lane in kanbanLanes"
        :key="lane.key"
        :class="$style.kanbanLane"
      >
        <header :class="$style.kanbanLaneHeader">
          <h2>{{ lane.title }}</h2>
          <span>{{ lane.rows.length || lane.items.length }}</span>
        </header>
        <div :class="$style.kanbanCards">
          <button
            v-for="row in lane.rows"
            :key="String(row.id)"
            type="button"
            :class="$style.kanbanCard"
            @click="openDetail(row)"
          >
            <strong>{{ row.code }}</strong>
            <span>{{ row.name || row.title }}</span>
            <small>{{ row.progress || row.metric }} · {{ row.owner || row.status }}</small>
          </button>
          <button
            v-for="(item, index) in lane.rows.length ? [] : lane.items"
            :key="`${lane.key}-item-${index}`"
            type="button"
            :class="$style.kanbanCard"
            @click="notify(`已打开：${item}`)"
          >
            <span>{{ item }}</span>
          </button>
        </div>
      </div>
    </section>

    <section
      v-if="isTargetBoardLayout"
      :class="$style.targetBoard"
      data-module-target-board
    >
      <div :class="$style.targetColumns">
        <article :class="$style.targetColumn">
          <header :class="$style.panelHeader">
            <h2>各团队业绩目标对比</h2>
          </header>
          <button
            v-for="row in targetTeamRows"
            :key="String(row.id)"
            type="button"
            :class="$style.targetRow"
            @click="openDetail(row)"
          >
            <div>
              <strong>{{ row.name || row.title }}</strong>
              <span>{{ row.owner }} · {{ row.status }}</span>
            </div>
            <div :class="$style.targetMetrics">
              <em>{{ row.progress }}</em>
              <small>{{ row.value }} / {{ row.metric }}</small>
            </div>
            <div :class="$style.progressTrack">
              <span :class="[$style.progressFill, progressLevel(String(row.progress || '0'))]" />
            </div>
          </button>
        </article>
        <article :class="$style.targetColumn">
          <header :class="$style.panelHeader">
            <h2>个人业绩贡献榜</h2>
          </header>
          <button
            v-for="row in targetPersonRows"
            :key="String(row.id)"
            type="button"
            :class="$style.targetRow"
            @click="notify(`查看 ${row.name || row.title}`)"
          >
            <div>
              <strong>{{ row.name || row.title }}</strong>
              <span>{{ row.type || '个人目标' }}</span>
            </div>
            <em>{{ row.progress || row.value || '—' }}</em>
          </button>
        </article>
      </div>
      <div
        v-if="boardPanels.length"
        :class="$style.targetPanels"
      >
        <article
          v-for="panel in boardPanels"
          :key="panel.key"
          :class="$style.dashboardPanel"
        >
          <header :class="$style.panelHeader">
            <h2>{{ panel.title }}</h2>
          </header>
          <ul :class="$style.panelList">
            <li
              v-for="(item, index) in panel.items"
              :key="`${panel.key}-${index}`"
            >
              <button
                type="button"
                :class="$style.panelItem"
                @click="notify(`已打开：${item}`)"
              >
                <span>{{ item }}</span>
                <small>查看</small>
              </button>
            </li>
          </ul>
        </article>
      </div>
    </section>

    <section
      v-if="isScheduleLayout"
      :class="$style.scheduleBoard"
      data-module-schedule
      data-schedule-mode="engineer-week"
    >
      <header :class="$style.scheduleHeader">
        <div>
          <h2>工程师 × 日期视图</h2>
          <p>一眼看清排程冲突、待预约和负载分布 · 7/13 - 7/19</p>
        </div>
        <div :class="$style.scheduleDayHeads">
          <span
            v-for="day in scheduleDays"
            :key="day.key"
            :data-day-iso="day.iso"
          >
            {{ day.label }} 周{{ day.weekday }}
            <small>{{ day.iso }}</small>
          </span>
        </div>
      </header>
      <div :class="$style.scheduleGrid">
        <div
          v-for="person in schedulePeople"
          :key="person.name"
          :class="$style.scheduleRow"
          :data-engineer="person.name"
        >
          <div :class="$style.schedulePerson">
            <strong>{{ person.name }}</strong>
            <span>{{ person.team }}</span>
            <small>{{ person.status }}</small>
          </div>
          <div :class="$style.scheduleCells">
            <button
              v-for="day in scheduleDays"
              :key="`${person.name}-${day.iso}`"
              type="button"
              :class="$style.scheduleCell"
              :data-day-iso="day.iso"
              :data-chip-count="eventsForScheduleDay(person.name, day.iso).length"
              @click="notify(`${person.name} · ${day.label}`)"
            >
              <span
                v-for="row in eventsForScheduleDay(person.name, day.iso)"
                :key="String(row.id)"
                :class="$style.scheduleChip"
                :data-event-date="row.date"
              >
                {{ row.value || row.type || row.name || '排程' }}
              </span>
            </button>
          </div>
        </div>
      </div>
      <div
        v-if="scheduleUnscheduled.length"
        :class="$style.unscheduledBlock"
        data-module-unscheduled
      >
        <header :class="$style.panelHeader">
          <h2>未排期（已派工·待约时间）</h2>
          <small>{{ scheduleUnscheduled.length }}</small>
        </header>
        <button
          v-for="row in scheduleUnscheduled"
          :key="String(row.id)"
          type="button"
          :class="$style.unscheduledItem"
          :data-ticket="row.code"
          @click="openDetail(row)"
        >
          <strong>{{ row.code }} · {{ row.name || row.title }}</strong>
          <span>{{ row.owner }} · {{ row.priority || row.status }}</span>
        </button>
      </div>
    </section>

    <section
      v-if="isMonthCalendarLayout"
      :class="$style.monthCalendar"
      data-module-month-calendar
      data-schedule-mode="month-leave"
      :data-month-label="monthMeta.label"
    >
      <header :class="$style.monthHeader">
        <div>
          <h2>{{ monthMeta.label }}</h2>
          <p>查看团队请假/出差排期，识别人力冲突</p>
        </div>
        <div :class="$style.monthLegend">
          <span>年假</span>
          <span>病假</span>
          <span>事假</span>
          <span>出差</span>
          <span>调休</span>
        </div>
      </header>
      <div :class="$style.monthWeekdays">
        <span
          v-for="label in ['一', '二', '三', '四', '五', '六', '日']"
          :key="label"
          >{{ label }}</span
        >
      </div>
      <div :class="$style.monthGrid">
        <button
          v-for="cell in monthCells"
          :key="cell.key"
          type="button"
          :class="[$style.monthCell, !cell.inMonth && $style.monthCellMuted]"
          :data-day-iso="cell.iso"
          :data-in-month="cell.inMonth ? '1' : '0'"
          :data-event-count="cell.events.length"
          @click="notify(`${cell.iso} · ${cell.events.length} 项`)"
        >
          <strong>{{ cell.day }}</strong>
          <span
            v-for="row in cell.events.slice(0, 3)"
            :key="String(row.id)"
            :class="$style.monthChip"
            :data-event-date="row.date"
            :data-event-type="row.type"
          >
            {{ row.owner || row.name }}
          </span>
          <small v-if="cell.events.length > 3">+{{ cell.events.length - 3 }}</small>
        </button>
      </div>
      <div :class="$style.monthOverview">
        <header :class="$style.panelHeader">
          <h2>本月概览</h2>
        </header>
        <ul :class="$style.panelList">
          <li
            v-for="row in filteredRows"
            :key="String(row.id)"
          >
            <button
              type="button"
              :class="$style.panelItem"
              @click="openDetail(row)"
            >
              <span>{{ row.name }} · {{ row.date }}{{ row.endDate ? `~${row.endDate}` : '' }}</span>
              <small>{{ row.dept || row.metric }}</small>
            </button>
          </li>
        </ul>
      </div>
    </section>

    <div
      v-if="page.tabs.length && !isLoginCardsLayout"
      :class="$style.toolbar"
    >
      <div :class="$style.tabs">
        <button
          v-for="tab in page.tabs"
          :key="tab"
          type="button"
          :class="[$style.tab, activeTab === tab && $style.tabActive]"
          @click="activeTab = tab"
        >
          {{ tab }}
        </button>
      </div>
      <label
        v-if="
          isListSurface ||
          isCardsLayout ||
          isBoardLayout ||
          isKanbanLayout ||
          isTargetBoardLayout ||
          isScheduleLayout ||
          isMonthCalendarLayout
        "
        :class="$style.searchField"
      >
        <Search
          :size="14"
          :stroke-width="1.8"
          :class="$style.searchIcon"
        />
        <input
          v-model="query"
          :placeholder="page.searchPlaceholder"
          :class="$style.searchInput"
        />
        <button
          v-if="query"
          type="button"
          :class="$style.clearButton"
          aria-label="清除搜索"
          @click="query = ''"
        >
          <X
            :size="12"
            :stroke-width="1.8"
          />
        </button>
      </label>
    </div>

    <div
      v-if="isListSurface || isCardsLayout || isKanbanLayout || isScheduleLayout || isMonthCalendarLayout"
      :class="$style.filterBar"
    >
      <div :class="$style.filters">
        <div
          v-for="filter in page.filters"
          :key="filter.key"
          :class="$style.filterField"
          role="group"
          :aria-label="filter.label"
        >
          <Filter
            :size="12"
            :stroke-width="1.8"
          />
          <span>{{ filter.label }}</span>
          <UiSelect
            :model-value="filterValues[filter.key] ?? filter.options[0] ?? ''"
            :class="$style.filterSelect"
            :label="filter.label"
            :options="filterOptions(filter.options)"
            size="sm"
            @update:model-value="filterValues[filter.key] = $event"
          />
        </div>
      </div>
      <div :class="$style.filterMeta">
        <span>共 {{ filteredRows.length }} 条记录</span>
        <span v-if="selectedCount">已选 {{ selectedCount }}</span>
        <button
          type="button"
          :class="$style.metaButton"
          @click="runTool('列设置')"
        >
          <Columns3
            :size="13"
            :stroke-width="1.8"
          />
          列设置
        </button>
        <button
          type="button"
          :class="$style.metaButton"
          @click="resetFilters"
        >
          <RefreshCw
            :size="13"
            :stroke-width="1.8"
          />
          重置
        </button>
      </div>
    </div>

    <div
      v-if="isListSurface"
      :class="$style.tableShell"
      data-module-table
    >
      <table :class="$style.table">
        <thead>
          <tr>
            <th :class="$style.checkCol">
              <UiCheckbox
                :model-value="allSelected"
                label="全选"
                @update:model-value="toggleAll"
              />
            </th>
            <th
              v-for="column in dataColumns"
              :key="column.key"
              :class="[
                $style.columnHead,
                column.width >= 180
                  ? $style.columnWide
                  : column.width >= 140
                    ? $style.columnMedium
                    : $style.columnNarrow,
              ]"
            >
              <button
                type="button"
                :class="$style.sortButton"
                @click="setSort(column.key)"
              >
                {{ column.title }}
                <span
                  v-if="sortKey === column.key"
                  :class="$style.sortMark"
                  >{{ sortDir === 'asc' ? '↑' : '↓' }}</span
                >
              </button>
            </th>
            <th :class="$style.actionCol">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in pagedRows"
            :key="String(row.id)"
            :class="selected.has(String(row.id)) && $style.rowSelected"
          >
            <td>
              <UiCheckbox
                :model-value="selected.has(String(row.id))"
                :label="`选择 ${row.name || row.title || row.code}`"
                @update:model-value="toggleRow(String(row.id))"
              />
            </td>
            <td
              v-for="column in dataColumns"
              :key="`${row.id}-${column.key}`"
            >
              <span
                v-if="column.key === 'status'"
                :class="[$style.status, $style[statusTone(String(row.status ?? ''))]]"
                >{{ formatCell(row, column.key) }}</span
              >
              <span
                v-else-if="column.key === 'traffic' || column.title === '红绿灯'"
                :class="$style.traffic"
                :data-tone="trafficTone(String(row[column.key] ?? row.status ?? ''))"
                aria-hidden="true"
              />
              <button
                v-else-if="column.key === 'name' || column.key === 'title' || column.key === 'code'"
                type="button"
                :class="$style.cellLink"
                @click="openDetail(row)"
              >
                {{ formatCell(row, column.key) }}
              </button>
              <span v-else>{{ formatCell(row, column.key) }}</span>
            </td>
            <td>
              <div :class="$style.rowActions">
                <button
                  v-if="rowActions.includes('view')"
                  type="button"
                  :class="$style.linkButton"
                  @click="openDetail(row)"
                >
                  查看
                </button>
                <button
                  v-if="rowActions.includes('edit')"
                  type="button"
                  :class="$style.linkButton"
                  @click="openEdit(row)"
                >
                  编辑
                </button>
                <button
                  v-if="rowActions.includes('copy')"
                  type="button"
                  :class="$style.linkButton"
                  @click="openCopy(row)"
                >
                  复制
                </button>
                <button
                  v-if="rowActions.includes('delete')"
                  type="button"
                  :class="[$style.linkButton, $style.linkDanger]"
                  @click="askDelete(row)"
                >
                  删除
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="!pagedRows.length">
            <td
              :colspan="dataColumns.length + 2"
              :class="$style.empty"
            >
              <div
                :class="$style.emptyState"
                data-domain-empty
              >
                <Package
                  :size="28"
                  :stroke-width="1.8"
                />
                <strong>暂无匹配数据</strong>
                <p>{{ page.emptyHint || '可调整筛选、重置条件，或新建一条演示记录' }}</p>
                <div :class="$style.emptyActions">
                  <UiButton
                    size="sm"
                    variant="secondary"
                    @click="resetFilters"
                    >重置筛选</UiButton
                  >
                  <UiButton
                    size="sm"
                    variant="primary"
                    @click="openCreate"
                    >{{ page.primaryAction }}</UiButton
                  >
                </div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer
      v-if="
        isListSurface ||
        isCardsLayout ||
        isBoardLayout ||
        isKanbanLayout ||
        isTargetBoardLayout ||
        isScheduleLayout ||
        isMonthCalendarLayout
      "
      :class="$style.footer"
    >
      <div
        v-if="isListSurface"
        :class="$style.batchBar"
      >
        <UiButton
          size="sm"
          :disabled="!selectedCount"
          @click="runTool('批量处理')"
          >批量处理</UiButton
        >
        <UiButton
          size="sm"
          variant="secondary"
          :disabled="!selectedCount"
          @click="runTool('导出')"
          >导出所选</UiButton
        >
        <UiButton
          size="sm"
          variant="danger"
          :disabled="!selectedCount"
          @click="askDelete()"
          >删除</UiButton
        >
      </div>
      <div
        v-if="isListSurface"
        :class="$style.pagination"
        data-domain-pagination
      >
        <label :class="$style.pageSizeField">
          <span>每页</span>
          <UiSelect
            :model-value="String(pageSize)"
            :options="PAGE_SIZE_OPTIONS"
            label="每页条数"
            size="sm"
            @update:model-value="setPageSize(Number($event))"
          />
        </label>
        <span :class="$style.pageMeta">第 {{ pageIndex }} / {{ totalPages }} 页 · {{ filteredRows.length }} 条</span>
        <button
          type="button"
          :class="$style.pageButton"
          :disabled="pageIndex <= 1"
          @click="setPage(pageIndex - 1)"
        >
          上一页
        </button>
        <button
          type="button"
          :class="$style.pageButton"
          :disabled="pageIndex >= totalPages"
          @click="setPage(pageIndex + 1)"
        >
          下一页
        </button>
      </div>
      <span :class="$style.footerMeta"
        >{{ page.module }} · {{ page.label }} · 本地演示数据 · {{ rows.length }} 条底稿</span
      >
    </footer>

    <UiDialog
      v-model:open="detailOpen"
      :title="String(activeRow?.title || activeRow?.name || page.label)"
      :description="`${page.module} / ${page.label}`"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailList"
      >
        <div
          v-for="column in allDataColumns"
          :key="column.key"
          :class="$style.detailItem"
        >
          <dt>{{ column.title }}</dt>
          <dd>{{ formatCell(activeRow, column.key) }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
        <UiButton
          variant="secondary"
          @click="
            detailOpen = false;
            if (activeRow) openCopy(activeRow);
          "
          >复制</UiButton
        >
        <UiButton
          variant="primary"
          @click="
            detailOpen = false;
            if (activeRow) openEdit(activeRow);
          "
        >
          <template #leading>
            <Check
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          编辑
        </UiButton>
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="createOpen"
      :title="activeRow ? `编辑${page.label}` : page.primaryAction"
      :description="`在 ${page.label} 中维护本地演示记录`"
      size="md"
    >
      <div :class="$style.formGrid">
        <label
          v-for="key in primaryFieldKeys()"
          :key="key"
          :class="$style.formField"
        >
          <span>{{ allDataColumns.find((column) => column.key === key)?.title || key }}</span>
          <input
            v-model="draft[key]"
            :placeholder="`请输入${allDataColumns.find((column) => column.key === key)?.title || key}`"
          />
        </label>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="createOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="createRecord"
          >保存</UiButton
        >
      </template>
    </UiDialog>

    <UiAlertDialog
      v-model:open="confirmOpen"
      title="确认操作"
      :description="pendingAction || '确认执行该操作？'"
      confirm-label="确认"
      tone="danger"
      @confirm="confirmPending"
    >
      <p :class="$style.confirmText">该操作仅影响本地演示数据，可继续安全验证交互。</p>
    </UiAlertDialog>

    <UiDialog
      v-model:open="columnOpen"
      title="列设置"
      :description="`自定义「${page.label}」表格可见列`"
      size="sm"
    >
      <div
        :class="$style.columnList"
        data-domain-column-settings
      >
        <label
          v-for="column in allDataColumns"
          :key="column.key"
          :class="$style.columnItem"
        >
          <UiCheckbox
            :model-value="dataColumns.some((item) => item.key === column.key)"
            :label="column.title"
            @update:model-value="toggleColumn(column.key)"
          />
          <span>{{ column.title }}</span>
        </label>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="resetColumns"
          >恢复默认</UiButton
        >
        <UiButton
          variant="secondary"
          @click="showAllColumns"
          >全部显示</UiButton
        >
        <UiButton
          variant="primary"
          @click="columnOpen = false"
          >完成</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="importOpen"
      title="导入演示数据"
      :description="`向「${page.label}」导入 3 条本地演示记录`"
      size="sm"
    >
      <p :class="$style.confirmText">导入不会请求服务器，仅用于验收表格、筛选与批量操作。</p>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="importOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="importDemoRows"
          >确认导入</UiButton
        >
      </template>
    </UiDialog>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply flex h-full min-h-0 flex-col gap-3 overflow-auto p-4;
  background: #fafbfc;
}

.hero {
  @apply flex items-start justify-between gap-4 rounded-lg border bg-white px-4 py-3;
  border-color: var(--ui-border);
}

.heroCopy {
  @apply min-w-0;
}

.breadcrumb {
  @apply mb-1 flex items-center gap-1 text-[12px];
  color: var(--ui-text-subtle);
}

.breadcrumbSep {
  color: #c1c7d0;
}

.title {
  @apply m-0 text-[20px] font-semibold leading-7;
  color: var(--ui-text);
}

.description {
  @apply m-0 mt-1 text-[13px] leading-5;
  color: var(--ui-text-muted);
}

.heroActions {
  @apply flex shrink-0 flex-wrap items-center justify-end gap-2;
}

.stats {
  @apply grid gap-3;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
}

.statCard {
  @apply rounded-lg border bg-white px-3 py-3 text-left transition-colors;
  border-color: var(--ui-border);
}

.statCard:hover {
  background: #f4f5f7;
}

.statCardActive {
  border-color: #b3d4ff;
  background: #deebff;
}

.statLabel {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}

.statValue {
  @apply mt-1 block text-[18px] font-semibold leading-6;
  color: var(--ui-text);
}

.statPrimary {
  box-shadow: inset 3px 0 0 var(--app-primary);
}
.statSuccess {
  box-shadow: inset 3px 0 0 var(--ui-success);
}
.statWarning {
  box-shadow: inset 3px 0 0 var(--ui-warning);
}
.statDanger {
  box-shadow: inset 3px 0 0 var(--ui-danger);
}
.statNeutral {
  box-shadow: inset 3px 0 0 #97a0af;
}

.dashboardGrid {
  @apply grid gap-3;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.dashboardPanel,
.calendarBoard,
.boardFeed {
  @apply rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.boardFeed {
  @apply flex flex-col gap-2;
}

.boardFeedList {
  @apply flex flex-col gap-1;
}

.boardFeedItem {
  @apply flex w-full items-center justify-between gap-3 rounded border-0 px-2 py-2 text-left transition-colors;
  background: #fafbfc;
  color: var(--ui-text);
}
.boardFeedItem:hover {
  background: #f4f5f7;
}
.boardFeedItem strong {
  @apply block text-[13px] font-medium;
}
.boardFeedItem span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.boardFeedItem small {
  @apply shrink-0 text-[12px] font-medium;
  color: var(--app-primary);
}

.cardGrid {
  @apply grid gap-3;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.entityCard {
  @apply flex flex-col gap-3 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.entityCardHeader {
  @apply flex items-start gap-2;
}
.entityCardHeader h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}
.entityCardHeader p {
  @apply m-0 mt-0.5 text-[12px];
  color: var(--ui-text-subtle);
}

.entityCardMeta {
  @apply m-0 grid grid-cols-2 gap-2;
}
.entityCardMeta div {
  @apply rounded px-2 py-1.5;
  background: #f4f5f7;
}
.entityCardMeta dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.entityCardMeta dd {
  @apply m-0 mt-0.5 text-[13px] font-medium;
  color: var(--ui-text);
}

.entityCardActions {
  @apply flex items-center gap-3 border-t pt-2;
  border-color: var(--ui-border-subtle);
}

.unscheduledItem {
  @apply flex w-full flex-col items-start gap-0.5 rounded border-0 px-3 py-2 text-left transition-colors;
  background: var(--ui-surface-subtle);
  color: var(--ui-text);
}

.unscheduledItem:hover {
  background: var(--app-primary-soft);
}

.unscheduledItem strong {
  @apply text-[13px] font-semibold;
  color: var(--app-primary);
}

.unscheduledItem span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.panelHeader {
  @apply mb-2 flex items-center gap-2 text-[13px] font-semibold;
  color: var(--ui-text);
}

.panelHeader h2 {
  @apply m-0 text-[13px] font-semibold;
}

.panelList {
  @apply m-0 flex list-none flex-col gap-1 p-0;
}

.panelItem {
  @apply flex w-full items-center justify-between rounded border-0 px-2 py-2 text-left text-[12px] transition-colors;
  color: var(--ui-text);
  background: transparent;
}
.panelItem:hover {
  background: var(--ui-surface-hover);
}
.panelItem small {
  color: var(--ui-text-subtle);
}

.calendarGrid {
  @apply grid gap-2;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
}

.calendarCard {
  @apply flex flex-col items-start gap-1 rounded border px-3 py-2 text-left transition-colors;
  border-color: var(--ui-border);
  background: #fff;
  color: var(--ui-text);
}
.calendarCard:hover {
  background: var(--ui-surface-hover);
}
.calendarCard strong {
  @apply text-[13px];
}
.calendarCard span,
.calendarCard small {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.loginSurface {
  @apply flex flex-col gap-4 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}

.loginHero h2 {
  @apply m-0 text-[18px] font-semibold;
  color: var(--ui-text);
}
.loginHero p {
  @apply m-0 mt-1 text-[13px];
  color: var(--ui-text-muted);
}

.loginGroup {
  @apply flex flex-col gap-2;
}

.loginGrid {
  @apply grid gap-3;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
}

.loginCard {
  @apply flex flex-col items-start gap-1 rounded-lg border px-3 py-3 text-left transition-colors active:scale-[.97];
  border-color: var(--ui-border);
  background: #fafbfc;
  color: var(--ui-text);
}
.loginCard:hover {
  background: #deebff;
  border-color: #b3d4ff;
}
.loginCode {
  @apply text-[12px] font-medium;
  color: var(--app-primary);
}
.loginCard strong {
  @apply text-[15px] font-semibold;
}
.loginCard small,
.loginCard em {
  @apply text-[12px] not-italic;
  color: var(--ui-text-subtle);
}

.kanbanBoard {
  @apply grid min-h-[360px] gap-3;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.kanbanLane {
  @apply flex min-h-[320px] flex-col gap-2 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.kanbanLaneHeader {
  @apply flex items-center justify-between border-b pb-2;
  border-color: var(--ui-border-subtle);
}
.kanbanLaneHeader h2 {
  @apply m-0 text-[13px] font-semibold;
  color: var(--ui-text);
}
.kanbanLaneHeader span {
  @apply rounded px-2 py-0.5 text-[12px];
  background: #deebff;
  color: var(--app-primary);
}

.kanbanCards {
  @apply flex flex-1 flex-col gap-2 overflow-auto;
}

.kanbanCard {
  @apply flex w-full flex-col items-start gap-1 rounded border px-3 py-2 text-left transition-colors;
  border-color: var(--ui-border);
  background: #fafbfc;
  color: var(--ui-text);
}
.kanbanCard:hover {
  background: #f4f5f7;
}
.kanbanCard strong {
  @apply text-[12px] font-semibold;
  color: var(--app-primary);
}
.kanbanCard span {
  @apply text-[13px];
}
.kanbanCard small {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.targetBoard {
  @apply flex flex-col gap-3;
}

.targetColumns {
  @apply grid gap-3;
  grid-template-columns: minmax(0, 1.4fr) minmax(0, 1fr);
}

.targetColumn {
  @apply flex flex-col gap-2 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.targetRow {
  @apply flex w-full flex-col gap-1 rounded border-0 px-2 py-2 text-left transition-colors;
  background: #fafbfc;
  color: var(--ui-text);
}
.targetRow:hover {
  background: #f4f5f7;
}
.targetRow strong {
  @apply block text-[13px] font-semibold;
}
.targetRow span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.targetRow em {
  @apply text-[14px] font-semibold not-italic;
  color: var(--app-primary);
}

.targetMetrics {
  @apply flex items-baseline justify-between gap-2;
}
.targetMetrics small {
  @apply text-[12px];
  color: var(--ui-text-muted);
}

.progressTrack {
  @apply mt-1 h-1.5 w-full overflow-hidden rounded-full;
  background: #ebecf0;
}
.progressFill {
  @apply block h-full rounded-full;
  background: var(--app-primary);
}
.progress0 {
  width: 0%;
}
.progress10 {
  width: 10%;
}
.progress25 {
  width: 25%;
}
.progress40 {
  width: 40%;
}
.progress60 {
  width: 60%;
}
.progress80 {
  width: 80%;
}
.progress100 {
  width: 100%;
}

.targetPanels {
  @apply grid gap-3;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
}

.scheduleBoard {
  @apply flex flex-col gap-3 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.scheduleHeader {
  @apply flex flex-wrap items-end justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border-subtle);
}
.scheduleHeader h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}
.scheduleHeader p {
  @apply m-0 mt-1 text-[12px];
  color: var(--ui-text-muted);
}

.scheduleDayHeads {
  @apply grid min-w-[520px] flex-1 gap-1;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}
.scheduleDayHeads span {
  @apply rounded px-1 py-1 text-center text-[12px] font-medium;
  background: #f4f5f7;
  color: var(--ui-text-muted);
}
.scheduleDayHeads small {
  @apply block text-[11px] font-normal;
}

.scheduleGrid {
  @apply flex flex-col gap-2 overflow-auto;
}

.scheduleRow {
  @apply grid gap-2;
  grid-template-columns: 140px minmax(0, 1fr);
}

.schedulePerson {
  @apply flex flex-col gap-0.5 rounded px-2 py-2;
  background: #f4f5f7;
}
.schedulePerson strong {
  @apply text-[13px];
  color: var(--ui-text);
}
.schedulePerson span,
.schedulePerson small {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.scheduleCells {
  @apply grid min-w-[520px] gap-1;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.scheduleCell {
  @apply flex min-h-[56px] flex-col gap-1 rounded border px-1 py-1 text-left transition-colors;
  border-color: var(--ui-border-subtle);
  background: #fafbfc;
}
.scheduleCell:hover {
  background: #deebff;
}

.scheduleChip {
  @apply rounded px-1 py-0.5 text-[11px];
  background: #deebff;
  color: var(--app-primary);
}

.unscheduledBlock {
  @apply flex flex-col gap-1 border-t pt-3;
  border-color: var(--ui-border-subtle);
}

.monthCalendar {
  @apply flex flex-col gap-3 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.monthHeader {
  @apply flex flex-wrap items-end justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border-subtle);
}
.monthHeader h2 {
  @apply m-0 text-[16px] font-semibold;
  color: var(--ui-text);
}
.monthHeader p {
  @apply m-0 mt-1 text-[12px];
  color: var(--ui-text-muted);
}

.monthLegend {
  @apply flex flex-wrap gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.monthLegend span {
  @apply rounded px-2 py-0.5;
  background: #f4f5f7;
}

.monthWeekdays {
  @apply grid gap-1 text-center text-[12px] font-medium;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  color: var(--ui-text-muted);
}

.monthGrid {
  @apply grid gap-1;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.monthCell {
  @apply flex min-h-[88px] flex-col items-start gap-0.5 rounded border px-1.5 py-1 text-left transition-colors;
  border-color: var(--ui-border-subtle);
  background: #fafbfc;
  color: var(--ui-text);
}
.monthCell:hover {
  background: #deebff;
}
.monthCell strong {
  @apply text-[12px] font-semibold;
}
.monthCellMuted {
  opacity: 0.45;
}

.monthChip {
  @apply max-w-full truncate rounded px-1 py-0.5 text-[11px];
  background: #deebff;
  color: var(--app-primary);
}
.monthCell small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.monthOverview {
  @apply rounded border p-2;
  border-color: var(--ui-border-subtle);
  background: #fafbfc;
}

.toolbar {
  @apply flex flex-wrap items-center justify-between gap-3 border-b pb-2;
  border-color: var(--ui-border);
}

.tabs {
  @apply flex min-w-0 flex-1 flex-wrap items-center gap-1;
}

.tab {
  @apply rounded border-0 bg-transparent px-3 py-1.5 text-[13px] transition-colors;
  color: var(--ui-text-muted);
}
.tab:hover {
  background: var(--ui-surface-hover);
  color: var(--ui-text);
}
.tabActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.searchField {
  @apply flex h-8 w-[280px] items-center gap-2 rounded border bg-white px-2;
  border-color: var(--ui-border);
}

.searchIcon {
  color: var(--ui-text-subtle);
}

.searchInput {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}

.clearButton {
  @apply inline-flex h-5 w-5 items-center justify-center rounded border-0 bg-transparent;
  color: var(--ui-text-subtle);
}

.filterBar {
  @apply flex flex-wrap items-center justify-between gap-2;
}

.filters {
  @apply flex flex-wrap items-center gap-2;
}

.filterField {
  @apply inline-flex items-center gap-1 text-[12px];
  color: var(--ui-text-muted);
}

.filterSelect {
  @apply min-w-28;
}

.filterMeta {
  @apply flex items-center gap-3 text-[12px];
  color: var(--ui-text-subtle);
}

.metaButton {
  @apply inline-flex items-center gap-1 rounded border-0 bg-transparent text-[12px] transition-colors;
  color: var(--ui-text-muted);
}
.metaButton:hover {
  color: var(--app-primary);
}

.tableShell {
  @apply min-h-0 flex-1 overflow-auto rounded-lg border bg-white;
  border-color: var(--ui-border);
}

.table {
  @apply w-full min-w-[960px] border-collapse text-left text-[12px];
}
.table th {
  @apply sticky top-0 z-[1] border-b px-3 py-2 font-medium;
  background: #f4f5f7;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}
.table td {
  @apply border-b px-3 py-2 align-middle;
  border-color: var(--ui-border-subtle);
  color: var(--ui-text);
}
.table tbody tr:hover {
  background: #fafbfc;
}

.rowSelected {
  background: var(--app-primary-soft) !important;
}

.checkCol {
  @apply w-10;
}
.actionCol {
  @apply w-28;
}
.columnHead {
  @apply whitespace-nowrap;
}
.columnWide {
  min-width: 180px;
}
.columnMedium {
  min-width: 140px;
}
.columnNarrow {
  min-width: 90px;
}

.sortButton {
  @apply inline-flex items-center gap-1 border-0 bg-transparent p-0 text-[12px] font-medium;
  color: inherit;
}

.sortMark {
  color: var(--app-primary);
}

.status {
  @apply inline-flex rounded px-2 py-0.5 text-[12px];
}
.success {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.danger {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.warning {
  color: #974f0c;
  background: var(--ui-warning-soft);
}
.neutral {
  color: var(--ui-text-muted);
  background: #f4f5f7;
}

.traffic {
  @apply inline-block h-2.5 w-2.5 rounded-full;
  background: #36b37e;
}
.traffic[data-tone='warning'] {
  background: #ff8b00;
}
.traffic[data-tone='danger'] {
  background: #de350b;
}
.traffic[data-tone='success'] {
  background: #36b37e;
}

.rowActions {
  @apply flex items-center gap-2;
}

.linkButton {
  @apply border-0 bg-transparent p-0 text-[12px] transition-colors;
  color: var(--app-primary);
}
.linkButton:hover {
  color: var(--app-primary-hover);
}

.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex flex-wrap items-center justify-between gap-3;
}

.batchBar {
  @apply flex flex-wrap items-center gap-2;
}

.pagination {
  @apply flex flex-wrap items-center gap-2;
}

.pageSizeField {
  @apply flex items-center gap-1 text-[12px];
  color: var(--ui-text-muted);
}

.pageMeta {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.pageButton {
  @apply rounded border bg-white px-2 py-1 text-[12px] transition-colors;
  border-color: var(--ui-border);
  color: var(--ui-text);
}

.pageButton:hover:not(:disabled) {
  background: #f4f5f7;
}

.pageButton:disabled {
  opacity: 0.45;
}

.footerMeta {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.cellLink {
  @apply border-0 bg-transparent p-0 text-left text-[13px] transition-colors;
  color: var(--app-primary);
}

.cellLink:hover {
  text-decoration: underline;
}

.linkDanger {
  color: var(--ui-danger);
}

.emptyState {
  @apply flex flex-col items-center justify-center gap-2 px-4 py-10 text-center;
  color: var(--ui-text-muted);
}

.emptyState strong {
  @apply text-[14px] font-medium;
  color: var(--ui-text);
}

.emptyState p {
  @apply m-0 max-w-md text-[12px] leading-5;
}

.emptyActions {
  @apply mt-2 flex flex-wrap items-center justify-center gap-2;
}

.columnList {
  @apply flex max-h-72 flex-col gap-1 overflow-auto;
}

.columnItem {
  @apply flex items-center gap-2 rounded px-2 py-1.5 text-[13px] transition-colors;
  color: var(--ui-text);
}

.columnItem:hover {
  background: #f4f5f7;
}

.detailList {
  @apply m-0 grid gap-2;
}

.detailItem {
  @apply grid grid-cols-[120px_1fr] gap-3 border-b py-2 text-[13px];
  border-color: var(--ui-border-subtle);
}
.detailItem dt {
  color: var(--ui-text-subtle);
}
.detailItem dd {
  @apply m-0;
  color: var(--ui-text);
}

.formGrid {
  @apply grid gap-3;
}

.formField {
  @apply flex flex-col gap-1 text-[12px];
  color: var(--ui-text-muted);
}
.formField input {
  @apply h-9 rounded border px-3 text-[13px] outline-none;
  border-color: var(--ui-border);
  color: var(--ui-text);
}
.formField input:focus {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--ui-focus) 25%, transparent);
}

.confirmText {
  @apply m-0 text-[13px] leading-6;
  color: var(--ui-text-muted);
}

.missing {
  @apply flex h-full items-center justify-center text-[13px];
  color: var(--ui-text-subtle);
}

@media (max-width: 1200px) {
  .dashboardGrid,
  .targetColumns,
  .kanbanBoard {
    grid-template-columns: 1fr;
  }

  .scheduleRow {
    grid-template-columns: 1fr;
  }

  .monthCell {
    min-height: 72px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .loginCard,
  .kanbanCard,
  .targetRow,
  .scheduleCell {
    transition: none;
  }
  .loginCard {
    transform: none;
  }
}
</style>
