<script setup lang="ts">
import { computed, ref } from 'vue';
import { Bell, Plus, QrCode, Search } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { certificatesFixture, issuedCertificates } from '../model/certificates.data';
import type { CertificateTemplate, CertificateView } from '../model/certificates.types';
import CertificatesWorkspace from './certificates/CertificatesWorkspace.vue';
import TrainingHero from './training-shared/TrainingHero.vue';
import TrainingNavigation from './training-shared/TrainingNavigation.vue';
const templates = ref<CertificateTemplate[]>(cloneReferenceRows(certificatesFixture.rows));
const view = ref<CertificateView>('证书模板');
const query = ref('');
const createOpen = ref(false);
const verifyOpen = ref(false);
const detailOpen = ref(false);
const activeTemplate = ref<CertificateTemplate | null>(null);
const title = ref('');
const verifyCode = ref('');
const verifyResult = ref('');
const visibleTemplates = computed(() => {
  const q = query.value.trim().toLowerCase();
  return templates.value.filter((item) => !q || `${item.title}${item.code}`.toLowerCase().includes(q));
});
function setView(value: string) {
  view.value = value as CertificateView;
}
function saveTemplate() {
  if (!title.value.trim()) {
    toast.warning('请填写证书模板名称');
    return;
  }
  templates.value = [
    ...templates.value,
    {
      id: `ct-${Date.now()}`,
      code: `CERT-2026-${String(templates.value.length + 1).padStart(3, '0')}`,
      title: title.value.trim(),
      issuer: 'Comap 人力资源部',
      tone: 'blue',
      issued: 0,
      valid: 0,
      expiring: 0,
      validity: '12 月',
      rule: '手动发证',
    },
  ];
  createOpen.value = false;
  toast.success('证书模板已保存到本地');
}
function verify() {
  const code = verifyCode.value.trim().toUpperCase();
  verifyResult.value =
    issuedCertificates.some((item) => item.id.toUpperCase() === code) ||
    templates.value.some((item) => item.code === code)
      ? '查验通过：证书有效'
      : '未找到匹配的证书记录';
}
function openTemplate(item: CertificateTemplate) {
  activeTemplate.value = item;
  detailOpen.value = true;
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/certificates"
    data-page-label="证书中心"
    data-page-module="人事管理"
    data-certificates-page
    :data-view="view"
    :data-template-count="templates.length"
  >
    <TrainingHero subtitle="9 张已发证书 · 3 张即将到期 · 自动复训预警"
      ><template #actions
        ><UiButton
          variant="inverted"
          size="sm"
          @click="createOpen = true"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建证书模板</UiButton
        ></template
      ></TrainingHero
    ><TrainingNavigation active="证书中心" />
    <section :class="$style.warning">
      <Bell
        :size="18"
        :stroke-width="1.8"
      />
      <p>
        <strong>全员共有 3 张证书即将到期，请安排相关学员重训</strong
        ><span>证书过期前 30~90 天系统会自动发送复训提醒，重训通过后自动续期</span>
      </p>
      <UiButton
        variant="warning"
        size="sm"
        @click="toast.success('已在本地通知 3 名学员')"
        >一键通知 3 人</UiButton
      >
    </section>
    <section :class="$style.controls">
      <UiTabs
        :model-value="view"
        @update:model-value="setView"
        ><UiTabsList label="证书中心视图"
          ><UiTabsTrigger value="证书模板">证书模板 ({{ templates.length }})</UiTabsTrigger
          ><UiTabsTrigger value="已发证书">已发证书 (9)</UiTabsTrigger
          ><UiTabsTrigger value="我的证书">我的证书 (3)</UiTabsTrigger></UiTabsList
        ></UiTabs
      >
      <div :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><UiInput
          v-model="query"
          placeholder="搜索模板名 / 编号"
          aria-label="搜索证书"
        />
      </div>
      <UiButton
        size="sm"
        @click="verifyOpen = true"
        ><template #leading
          ><QrCode
            :size="14"
            :stroke-width="1.8" /></template
        >扫码查验</UiButton
      >
    </section>
    <CertificatesWorkspace
      :view="view"
      :templates="visibleTemplates"
      :issued="issuedCertificates"
      :query="query"
      @open="openTemplate"
    />
  </main>
  <UiDialog
    v-model:open="createOpen"
    title="新建证书模板"
    description="模板仅保存到当前本地会话"
    size="md"
    ><div :class="$style.form">
      <UiFormField
        label="模板名称"
        required
        ><UiInput
          v-model="title"
          aria-label="证书模板名称"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="saveTemplate"
        >保存模板</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="verifyOpen"
    title="证书查验"
    description="输入证书编号验证本地证书记录"
    size="md"
    ><div :class="$style.verify">
      <UiInput
        v-model="verifyCode"
        placeholder="例如 CERT-2026-001"
        aria-label="证书查验编号"
      />
      <p v-if="verifyResult">{{ verifyResult }}</p>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="verifyOpen = false"
        >关闭</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="verify"
        >查验</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="activeTemplate?.title || '证书模板'"
    :description="activeTemplate ? `${activeTemplate.code} · ${activeTemplate.issuer}` : undefined"
    size="lg"
    ><dl
      v-if="activeTemplate"
      :class="$style.details"
    >
      <div>
        <dt>已发证</dt>
        <dd>{{ activeTemplate.issued }}</dd>
      </div>
      <div>
        <dt>有效中</dt>
        <dd>{{ activeTemplate.valid }}</dd>
      </div>
      <div>
        <dt>将到期</dt>
        <dd>{{ activeTemplate.expiring }}</dd>
      </div>
      <div>
        <dt>有效期</dt>
        <dd>{{ activeTemplate.validity }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>发证规则</dt>
        <dd>{{ activeTemplate.rule }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>
<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.warning {
  @apply mt-4 flex h-[60px] items-center gap-3 rounded border px-4;
  border-color: #ffbd85;
  background: #fff8ed;
}
.warning > svg {
  @apply box-content rounded p-2;
  color: white;
  background: var(--ui-warning);
}
.warning p {
  @apply m-0 flex flex-1 flex-col text-[10px];
  color: var(--ui-text-muted);
}
.warning strong {
  @apply text-[11px];
  color: var(--ui-text);
}
.controls {
  @apply mt-4 flex h-[54px] items-center gap-3 rounded border bg-white px-3;
  border-color: var(--ui-border);
}
.controls [role='tab'] {
  @apply h-9;
}
.search {
  @apply relative ml-auto w-[420px];
}
.search > svg {
  @apply absolute left-3 top-[10px] z-10;
  color: var(--ui-text-subtle);
}
.search input {
  @apply pl-9;
}
.form,
.verify {
  @apply p-5;
}
.verify p {
  @apply mt-3 rounded px-3 py-2 text-[10px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.details {
  @apply grid grid-cols-4 gap-4 p-5;
}
.details div {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply m-0 mt-2 text-[14px] font-semibold;
}
.wide {
  @apply col-span-4;
}
</style>
