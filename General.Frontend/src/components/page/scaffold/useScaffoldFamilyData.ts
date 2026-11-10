import { computed, type ComputedRef, type Ref } from 'vue';
import type { PageModel, PageRow } from '../page.types';

function isIsoDate(value: string) {
  return /^\d{4}-\d{2}-\d{2}$/.test(value);
}

function parseIso(value: string) {
  const parts = value.split('-').map(Number);
  const year = parts[0] ?? 2026;
  const month = parts[1] ?? 1;
  const day = parts[2] ?? 1;
  return new Date(year, month - 1, day);
}

function formatIso(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function eventCoversDate(row: PageRow, iso: string) {
  const start = String(row.date ?? '');
  if (!isIsoDate(start)) return false;
  const endRaw = String(row.endDate ?? row.date ?? '');
  const end = isIsoDate(endRaw) ? endRaw : start;
  return start <= iso && iso <= end;
}

export function useScaffoldFamilyData(page: Ref<PageModel>, filteredRows: ComputedRef<PageRow[]>) {
  const boardPanels = computed(() => page.value.panels ?? []);

  const kanbanLanes = computed(() => {
    const pending = filteredRows.value.filter(
      (row) => row.lane === 'pending' || String(row.type ?? '').includes('待派'),
    );
    const assigned = filteredRows.value.filter(
      (row) =>
        row.lane === 'assigned' ||
        String(row.type ?? '').includes('已派') ||
        String(row.status ?? '').includes('已派') ||
        String(row.status ?? '').includes('在制'),
    );
    const panels = page.value.panels ?? [];
    if (panels.length >= 2) {
      return [
        {
          key: panels[0]?.key ?? 'pool',
          title: panels[0]?.title ?? '待派工序池',
          rows: pending.length ? pending : filteredRows.value.filter((row) => !assigned.includes(row)),
          items: panels[0]?.items ?? [],
        },
        {
          key: panels[1]?.key ?? 'assigned',
          title: panels[1]?.title ?? '按工作中心已派工序',
          rows: assigned,
          items: panels[1]?.items ?? [],
        },
      ];
    }
    return [
      { key: 'pool', title: '待派工序池', rows: pending.length ? pending : filteredRows.value, items: [] as string[] },
      { key: 'assigned', title: '按工作中心已派工序', rows: assigned, items: [] as string[] },
    ];
  });

  const loginGroups = computed(() => {
    const groups = new Map<string, PageRow[]>();
    for (const row of filteredRows.value) {
      const department = String(row.dept || row.type || row.metric || '全部工位');
      const list = groups.get(department) ?? [];
      list.push(row);
      groups.set(department, list);
    }
    if (!groups.size) return [{ key: 'all', title: '全部工位', rows: filteredRows.value }];
    return [...groups.entries()].map(([title, groupRows], index) => ({
      key: `login-${index}`,
      title,
      rows: groupRows,
    }));
  });

  const scheduleUnscheduled = computed(() =>
    filteredRows.value.filter(
      (row) =>
        row.role === 'unscheduled' ||
        String(row.status ?? '').includes('未排') ||
        String(row.type ?? '').includes('待约'),
    ),
  );

  const schedulePeople = computed(() => {
    const resources = filteredRows.value.filter(
      (row) => row.role === 'resource' || (!row.role && !scheduleUnscheduled.value.includes(row)),
    );
    const source = resources.length
      ? resources
      : filteredRows.value.filter((row) => !scheduleUnscheduled.value.includes(row));
    return source.map((row) => ({
      name: String(row.owner || row.name || '未分派'),
      team: String(row.type || row.dept || row.metric || ''),
      status: String(row.status || '0 排程'),
    }));
  });

  const scheduleDays = computed(() => {
    const base = new Date(2026, 6, 13);
    return Array.from({ length: 7 }, (_, index) => {
      const day = new Date(base);
      day.setDate(base.getDate() + index);
      const iso = formatIso(day);
      const label = `${day.getMonth() + 1}/${day.getDate()}`;
      const weekday = ['一', '二', '三', '四', '五', '六', '日'][index] ?? '';
      return { key: iso, label, weekday, iso };
    });
  });

  const monthMeta = computed(() => {
    const dates = filteredRows.value
      .map((row) => String(row.date ?? ''))
      .filter((value) => isIsoDate(value))
      .sort();
    const anchor = dates[0] ? parseIso(dates[0]) : new Date(2026, 2, 1);
    return {
      year: anchor.getFullYear(),
      month: anchor.getMonth(),
      label: `${anchor.getFullYear()}年${anchor.getMonth() + 1}月`,
    };
  });

  const monthCells = computed(() => {
    const { year, month } = monthMeta.value;
    const first = new Date(year, month, 1);
    const startOffset = (first.getDay() + 6) % 7;
    const gridStart = new Date(year, month, 1 - startOffset);
    return Array.from({ length: 42 }, (_, index) => {
      const day = new Date(gridStart);
      day.setDate(gridStart.getDate() + index);
      const iso = formatIso(day);
      return {
        key: iso,
        iso,
        day: day.getDate(),
        inMonth: day.getMonth() === month,
        events: filteredRows.value.filter((row) => eventCoversDate(row, iso)),
      };
    });
  });

  function eventsForScheduleDay(personName: string, iso: string) {
    return filteredRows.value.filter(
      (row) =>
        String(row.owner || row.name || '') === personName &&
        isIsoDate(String(row.date ?? '')) &&
        eventCoversDate(row, iso),
    );
  }

  const targetTeamRows = computed(() => {
    const teams = filteredRows.value.filter(
      (row) => String(row.type ?? '').includes('团队') || String(row.code ?? '').startsWith('TEAM'),
    );
    return teams.length ? teams : filteredRows.value.slice(0, 4);
  });

  const targetPersonRows = computed(() => {
    const people = filteredRows.value.filter(
      (row) => String(row.type ?? '').includes('个人') || String(row.code ?? '').startsWith('P'),
    );
    if (people.length) return people;
    const personPanel = page.value.panels.find((panel) => panel.title.includes('个人') || panel.title.includes('贡献'));
    if (personPanel?.items?.length) {
      return personPanel.items.map((item, index) => {
        const progress = String(item).match(/[\d.]+%/)?.[0] ?? '—';
        return {
          id: `person-${index}`,
          name: item,
          title: item,
          progress,
          value: progress,
          type: '个人目标',
        } satisfies PageRow;
      });
    }
    return filteredRows.value.slice(0, 5);
  });

  return {
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
  };
}
