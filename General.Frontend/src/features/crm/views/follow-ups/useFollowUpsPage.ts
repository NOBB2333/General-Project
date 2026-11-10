import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { followUpOwners, followUpsReferenceFixture, followUpTypes } from '@/features/crm/model/follow-ups.data';
import type {
  FollowUpDraft,
  FollowUpRole,
  FollowUpRow,
  FollowUpScope,
  FollowUpType,
  FollowUpView,
} from '@/features/crm/model/follow-ups.types';

export function useFollowUpsPage() {
  const rows = ref<FollowUpRow[]>(cloneReferenceRows<FollowUpRow>(followUpsReferenceFixture.rows));
  const scope = ref<FollowUpScope>('all');
  const type = ref<FollowUpType | 'all'>('all');
  const owner = ref('all');
  const role = ref<FollowUpRole>('admin');
  const view = ref<FollowUpView>('list');
  const query = ref('');
  const editorOpen = ref(false);
  let nextId = 7;

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesScope =
        scope.value === 'all' || (scope.value === 'following' ? row.followed : row.ownerScope === scope.value);
      const matchesType = type.value === 'all' || row.type === type.value;
      const matchesOwner = owner.value === 'all' || row.owner === owner.value;
      const matchesRole = role.value !== 'sales' || row.ownerScope === 'mine';
      const matchesQuery =
        !keyword ||
        [row.customer, row.contact, row.content, row.result, row.opportunity ?? ''].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesScope && matchesType && matchesOwner && matchesRole && matchesQuery;
    });
  });

  const addedCount = computed(() => Math.max(0, rows.value.length - followUpsReferenceFixture.rows.length));
  const totalCount = computed(() => 36 + addedCount.value);
  const weekCount = computed(() => addedCount.value);
  const pendingCount = computed(() => 36 + addedCount.value);
  const customerCount = computed(() => 8 + addedCount.value);

  function saveFollowUp(draft: FollowUpDraft) {
    if (!draft.customer.trim() || !draft.contact.trim() || !draft.content.trim()) {
      toast.warning('请填写客户、联系人和跟进内容');
      return;
    }
    const id = `FU-LOCAL-${String(nextId).padStart(3, '0')}`;
    rows.value = [
      {
        id,
        date: '2026-09-16',
        type: draft.type,
        customer: draft.customer.trim(),
        contact: draft.contact.trim(),
        owner: draft.owner,
        content: draft.content.trim(),
        result: draft.result.trim() || '待补充跟进结果',
        nextDate: draft.nextDate || undefined,
        nextPlan: draft.nextPlan.trim() || undefined,
        ownerScope: draft.owner === '张明' ? 'mine' : 'subordinate',
      },
      ...rows.value,
    ];
    nextId += 1;
    editorOpen.value = false;
    toast.success('跟进记录已新增');
  }

  return {
    rows,
    scope,
    type,
    owner,
    role,
    view,
    query,
    editorOpen,
    filteredRows,
    totalCount,
    weekCount,
    pendingCount,
    customerCount,
    types: followUpTypes,
    owners: followUpOwners,
    saveFollowUp,
  };
}
