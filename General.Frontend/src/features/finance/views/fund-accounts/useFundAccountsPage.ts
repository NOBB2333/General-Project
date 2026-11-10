import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { fundAccountSummaryFixture, fundAccountsReferenceFixture } from '../../model/fund-accounts.data';
import type {
  FundAccount,
  FundAccountAction,
  FundAccountDraft,
  FundAccountKind,
  FundAccountStatus,
  FundAccountSummary,
  FundAccountTone,
} from '../../model/fund-accounts.types';

export type FundAccountLayout = 'cards' | 'table';

const emptyDraft = (): FundAccountDraft => ({
  name: '',
  kind: '银行账户',
  category: '一般户',
  status: '正常',
  company: '上海科迈自动化技术有限公司',
  holder: '',
  accountNumber: '',
  branch: '',
  balance: '0',
  purpose: '',
});

function toneForKind(kind: FundAccountKind): FundAccountTone {
  if (kind === '微信') return 'wechat';
  if (kind === '支付宝') return 'alipay';
  if (kind === '现金账户') return 'cash';
  if (kind === '银行账户' || kind === '承兑汇票账户') return 'bank';
  return 'other';
}

function escapeCsv(value: string | number) {
  const text = String(value).replace(/"/g, '""');
  return `"${text}"`;
}

export function useFundAccountsPage() {
  const accounts = ref<FundAccount[]>(cloneReferenceRows(fundAccountsReferenceFixture.rows));
  const query = ref('');
  const kind = ref('全部类型');
  const status = ref('全部状态');
  const layout = ref<FundAccountLayout>('cards');
  const operationMessage = ref('');

  const editorOpen = ref(false);
  const editingId = ref<string | null>(null);
  const accountDraft = ref<FundAccountDraft>(emptyDraft());

  const openingOpen = ref(false);
  const openingAccountId = ref('');
  const openingAmount = ref('');
  const openingDate = ref('2025-12-31');

  const detailOpen = ref(false);
  const detailAccountId = ref('');
  const deleteOpen = ref(false);
  const pendingDeleteId = ref('');

  const filteredAccounts = computed(() => {
    const keyword = query.value.trim().toLowerCase();
    return accounts.value.filter((account) => {
      const matchesKind = kind.value === '全部类型' || account.kind === kind.value;
      const matchesStatus = status.value === '全部状态' || account.status === status.value;
      const matchesQuery =
        !keyword ||
        [account.name, account.company, account.holder, account.accountNumber, account.branch, account.purpose].some(
          (value) => value.toLowerCase().includes(keyword),
        );
      return matchesKind && matchesStatus && matchesQuery;
    });
  });

  const missingOpeningAccounts = computed(() => accounts.value.filter((account) => !account.hasOpeningBalance));
  const detailAccount = computed(() => accounts.value.find((account) => account.id === detailAccountId.value) ?? null);
  const openingAccount = computed(
    () => accounts.value.find((account) => account.id === openingAccountId.value) ?? null,
  );
  const pendingDeleteAccount = computed(
    () => accounts.value.find((account) => account.id === pendingDeleteId.value) ?? null,
  );
  const summary = computed<FundAccountSummary>(() => ({
    totalBalance: accounts.value.reduce((total, account) => total + account.balance, 0),
    availableBalance: accounts.value.reduce(
      (total, account) => total + account.balance - (account.status === '冻结' ? account.frozenBalance : 0),
      0,
    ),
    activeCount: accounts.value.filter((account) => account.status !== '注销').length,
    totalCount: accounts.value.length,
    trend: fundAccountSummaryFixture.trend,
  }));

  function setOperation(message: string) {
    operationMessage.value = message;
  }

  function resetFilters() {
    query.value = '';
    kind.value = '全部类型';
    status.value = '全部状态';
    setOperation('已重置全部筛选条件');
  }

  function openCreate() {
    editingId.value = null;
    accountDraft.value = emptyDraft();
    editorOpen.value = true;
  }

  function openEdit(account: FundAccount) {
    editingId.value = account.id;
    accountDraft.value = {
      name: account.name,
      kind: account.kind,
      category: account.category,
      status: account.status,
      company: account.company,
      holder: account.holder,
      accountNumber: account.accountNumber,
      branch: account.branch,
      balance: String(account.balance),
      purpose: account.purpose,
    };
    editorOpen.value = true;
  }

  function saveAccount() {
    const draft = accountDraft.value;
    const name = draft.name.trim();
    if (!name || !draft.accountNumber.trim()) {
      setOperation('请填写账户名称和账号后再保存');
      return;
    }
    const balance = Number(draft.balance.replace(/,/g, '')) || 0;
    if (editingId.value) {
      accounts.value = accounts.value.map((account) =>
        account.id === editingId.value
          ? {
              ...account,
              ...draft,
              name,
              accountNumber: draft.accountNumber.trim(),
              balance,
              tone: toneForKind(draft.kind),
            }
          : account,
      );
      setOperation(`已保存账户「${name}」的修改`);
    } else {
      accounts.value = [
        {
          id: `account-local-${Date.now()}`,
          ...draft,
          name,
          accountNumber: draft.accountNumber.trim(),
          balance,
          frozenBalance: 0,
          hasOpeningBalance: false,
          tone: toneForKind(draft.kind),
        },
        ...accounts.value,
      ];
      setOperation(`已新增资金账户「${name}」`);
    }
    editorOpen.value = false;
  }

  function openDetail(account: FundAccount) {
    detailAccountId.value = account.id;
    detailOpen.value = true;
  }

  function openOpening(accountId?: string) {
    const target = accountId
      ? accounts.value.find((account) => account.id === accountId)
      : missingOpeningAccounts.value[0];
    if (!target) {
      setOperation('所有账户均已完成期初建账');
      return;
    }
    openingAccountId.value = target.id;
    openingAmount.value = target.openingBalance === undefined ? '' : String(target.openingBalance);
    openingDate.value = target.openingDate ?? '2025-12-31';
    openingOpen.value = true;
  }

  function saveOpeningBalance() {
    const amount = Number(openingAmount.value.replace(/,/g, ''));
    const account = openingAccount.value;
    if (!account || !Number.isFinite(amount) || amount < 0 || !openingDate.value) {
      setOperation('请填写有效的期初余额和日期');
      return;
    }
    accounts.value = accounts.value.map((item) =>
      item.id === account.id
        ? { ...item, openingBalance: amount, openingDate: openingDate.value, hasOpeningBalance: true }
        : item,
    );
    openingOpen.value = false;
    setOperation(`账户「${account.name}」已完成期初建账`);
  }

  function copyAccount(account: FundAccount) {
    const copy: FundAccount = {
      ...account,
      id: `account-copy-${Date.now()}`,
      name: `${account.name}（副本）`,
      accountNumber: `${account.accountNumber}-COPY`,
      hasOpeningBalance: false,
      openingBalance: undefined,
      openingDate: undefined,
    };
    accounts.value = [copy, ...accounts.value];
    setOperation(`已复制账户「${account.name}」`);
  }

  function toggleAccountStatus(account: FundAccount) {
    const nextStatus: FundAccountStatus = account.status === '冻结' ? '正常' : '冻结';
    accounts.value = accounts.value.map((item) => (item.id === account.id ? { ...item, status: nextStatus } : item));
    setOperation(`账户「${account.name}」已${nextStatus === '冻结' ? '冻结' : '恢复正常'}`);
  }

  function runAccountAction(action: FundAccountAction, account: FundAccount) {
    if (action === 'view') openDetail(account);
    else if (action === 'edit') openEdit(account);
    else if (action === 'opening') openOpening(account.id);
    else if (action === 'copy') copyAccount(account);
    else if (action === 'toggle-status') toggleAccountStatus(account);
    else {
      pendingDeleteId.value = account.id;
      deleteOpen.value = true;
    }
  }

  function confirmDelete() {
    const account = pendingDeleteAccount.value;
    if (!account) return;
    accounts.value = accounts.value.filter((item) => item.id !== account.id);
    deleteOpen.value = false;
    pendingDeleteId.value = '';
    setOperation(`已删除账户「${account.name}」`);
  }

  function exportAccounts() {
    const header = ['账户名称', '类型', '用途', '状态', '账号', '开户行', '余额', '持有人'];
    const rows = filteredAccounts.value.map((account) => [
      account.name,
      account.kind,
      account.category,
      account.status,
      account.accountNumber,
      account.branch,
      account.balance,
      account.holder,
    ]);
    const csv = [header, ...rows].map((row) => row.map(escapeCsv).join(',')).join('\n');
    const blob = new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = '资金账户.csv';
    link.click();
    URL.revokeObjectURL(url);
    setOperation(`已导出 ${rows.length} 个资金账户`);
  }

  return {
    summary,
    accounts,
    query,
    kind,
    status,
    layout,
    operationMessage,
    filteredAccounts,
    missingOpeningAccounts,
    editorOpen,
    editingId,
    accountDraft,
    openingOpen,
    openingAccountId,
    openingAmount,
    openingDate,
    openingAccount,
    detailOpen,
    detailAccount,
    deleteOpen,
    pendingDeleteAccount,
    resetFilters,
    openCreate,
    saveAccount,
    openOpening,
    saveOpeningBalance,
    runAccountAction,
    confirmDelete,
    exportAccounts,
  };
}
