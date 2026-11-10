import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { contactOwners, contactsReferenceFixture } from '@/features/crm/model/contacts-mgmt.data';
import type { ContactDraft, ContactRow, ContactStatus } from '@/features/crm/model/contacts-mgmt.types';

export function useContactsMgmtPage() {
  const rows = ref<ContactRow[]>(cloneReferenceRows<ContactRow>(contactsReferenceFixture.rows));
  const query = ref('');
  const status = ref<ContactStatus | 'all'>('all');
  const owner = ref('');
  const editorOpen = ref(false);
  let nextLocalNumber = 12;
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchStatus = status.value === 'all' || row.status === status.value;
      const matchOwner = !owner.value || row.owner === owner.value;
      const matchQuery =
        !keyword ||
        [row.name, row.position, row.department, row.company, row.companyExtra ?? '', row.phone, row.email].some(
          (value) => value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchStatus && matchOwner && matchQuery;
    });
  });
  const owners = computed(() => [...new Set([...contactOwners, ...rows.value.map((row) => row.owner)])]);
  const counts = computed<Record<'all' | ContactStatus, number>>(() => ({
    all: rows.value.length,
    在职: rows.value.filter((row) => row.status === '在职').length,
    已跳槽: rows.value.filter((row) => row.status === '已跳槽').length,
    已离职: rows.value.filter((row) => row.status === '已离职').length,
    停用: rows.value.filter((row) => row.status === '停用').length,
  }));
  const activeCount = computed(() => counts.value['在职']);
  const awayCount = computed(() => counts.value['已跳槽'] + counts.value['已离职']);
  const keyPersonCount = computed(
    () => rows.value.filter((row) => ['第一决策人', '关键决策人'].includes(row.decision)).length,
  );
  function saveContact(draft: ContactDraft) {
    if (!draft.name.trim() || !draft.company.trim()) {
      toast.warning('请填写姓名和所属公司');
      return;
    }
    rows.value = [
      {
        id: 'CON-LOCAL-' + String(nextLocalNumber).padStart(3, '0'),
        name: draft.name.trim(),
        position: draft.position.trim() || '-',
        department: draft.department.trim() || '-',
        company: draft.company,
        companyExtra: draft.companyExtra.trim() || undefined,
        status: draft.status,
        decision: draft.decision,
        phone: draft.phone.trim() || '-',
        email: draft.email.trim() || '-',
        owner: draft.owner,
        ownerInitial: draft.owner.slice(0, 1),
        isPrimary: draft.isPrimary,
      },
      ...rows.value,
    ];
    nextLocalNumber += 1;
    editorOpen.value = false;
    toast.success('已新增联系人「' + draft.name.trim() + '」');
  }
  return {
    rows,
    query,
    status,
    owner,
    editorOpen,
    filteredRows,
    owners,
    counts,
    activeCount,
    awayCount,
    keyPersonCount,
    saveContact,
  };
}
