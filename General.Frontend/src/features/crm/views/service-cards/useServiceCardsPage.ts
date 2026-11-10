import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { serviceCardCustomers, serviceCardsReferenceFixture } from '@/features/crm/model/service-cards.data';
import type { ServiceCardDraft, ServiceCardFilter, ServiceCardRow } from '@/features/crm/model/service-cards.types';

export function useServiceCardsPage() {
  const rows = ref<ServiceCardRow[]>(cloneReferenceRows<ServiceCardRow>(serviceCardsReferenceFixture.rows));
  const query = ref('');
  const customer = ref('');
  const status = ref<ServiceCardFilter>('all');
  const editorOpen = ref(false);
  let nextLocalNumber = 7;

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesStatus = status.value === 'all' || row.status === status.value;
      const matchesCustomer = !customer.value || row.customer === customer.value;
      const matchesKeyword =
        !keyword ||
        [row.serviceName, row.tierLabel, row.cardNumber, row.customer].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesStatus && matchesCustomer && matchesKeyword;
    });
  });
  const customers = computed(() => [...new Set([...serviceCardCustomers, ...rows.value.map((row) => row.customer)])]);
  const activeCount = computed(() => rows.value.filter((row) => row.status === 'active').length);
  const expiringCount = computed(() => rows.value.filter((row) => row.status === 'expiring').length);
  const expiredCount = computed(() => rows.value.filter((row) => row.status === 'expired').length);

  function saveCard(draft: ServiceCardDraft) {
    const amount = Number(draft.amount);
    if (
      !draft.serviceName.trim() ||
      !draft.customer.trim() ||
      !draft.cardNumber.trim() ||
      !draft.validUntil ||
      !Number.isFinite(amount) ||
      amount <= 0
    ) {
      toast.warning('请完整填写客户、服务、卡号、有效期和金额');
      return;
    }

    rows.value = [
      {
        id: 'SC-LOCAL-' + String(nextLocalNumber).padStart(4, '0'),
        serviceName: draft.serviceName.trim(),
        tierLabel: draft.tierLabel,
        cardNumber: draft.cardNumber.trim(),
        customer: draft.customer.trim(),
        validUntil: draft.validUntil,
        validityLabel: draft.validityLabel.trim() || '有效期 1 年',
        amount,
        status: 'active',
        remainingText: '剩余 365 天',
        progress: 0,
        tone: draft.tone,
      },
      ...rows.value,
    ];
    nextLocalNumber += 1;
    editorOpen.value = false;
    toast.success('已生成「' + draft.serviceName.trim() + '」权益卡');
  }

  return {
    rows,
    query,
    customer,
    status,
    editorOpen,
    filteredRows,
    customers,
    activeCount,
    expiringCount,
    expiredCount,
    saveCard,
  };
}
