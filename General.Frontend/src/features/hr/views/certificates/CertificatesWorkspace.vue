<script setup lang="ts">
import { Award, QrCode } from 'lucide-vue-next';
import type { CertificateTemplate, CertificateView, IssuedCertificate } from '../../model/certificates.types';
const props = defineProps<{
  view: CertificateView;
  templates: readonly CertificateTemplate[];
  issued: readonly IssuedCertificate[];
  query: string;
}>();
const emit = defineEmits<{ open: [item: CertificateTemplate] }>();
const issuedVisible = () => {
  const q = props.query.trim().toLowerCase();
  const source = props.view === '我的证书' ? props.issued.filter((item) => item.holder === '张明远') : props.issued;
  return source.filter(
    (item) => !q || `${item.templateTitle}${item.holder}${item.department}`.toLowerCase().includes(q),
  );
};
</script>
<template>
  <section :class="$style.workspace">
    <div
      v-if="view === '证书模板'"
      :class="$style.cards"
    >
      <article
        v-for="item in templates"
        :key="item.id"
        :data-tone="item.tone"
      >
        <div :class="$style.certificate">
          <header>
            <Award
              :size="16"
              :stroke-width="1.8"
            /><strong>Risemap · Comap</strong><span>SAMPLE</span>
          </header>
          <p>CERTIFICATE OF ACHIEVEMENT</p>
          <h2>{{ item.title }}</h2>
          <small>兹证明</small><b>张明远</b><small>（部门 / 岗位）</small>
          <p>圆满完成本课程相关学习与考核要求，特此认证</p>
          <dl>
            <div>
              <dt>颁发日期</dt>
              <dd>YYYY-MM-DD</dd>
            </div>
            <div>
              <dt>有效期至</dt>
              <dd>{{ item.validity }}</dd>
            </div>
            <div>
              <dt>证书编号</dt>
              <dd>COMAP-XXXX-####</dd>
            </div>
          </dl>
          <QrCode
            :size="22"
            :stroke-width="1.8"
          />
        </div>
        <div :class="$style.cardBody">
          <h3>{{ item.title }}</h3>
          <p>{{ item.code }} · {{ item.issuer }}</p>
          <div>
            <span
              >已发证<strong>{{ item.issued }}</strong></span
            ><span
              >有效中<strong>{{ item.valid }}</strong></span
            ><span
              >将到期<strong>{{ item.expiring }}</strong></span
            >
          </div>
          <p>
            <span>有效期 {{ item.validity }}</span
            ><span>{{ item.rule }}</span>
          </p>
          <button
            type="button"
            @click="emit('open', item)"
          >
            预览
          </button>
        </div>
      </article>
      <p
        v-if="templates.length === 0"
        :class="$style.empty"
      >
        没有匹配的证书模板
      </p>
    </div>
    <div
      v-else
      :class="$style.list"
    >
      <article
        v-for="item in issuedVisible()"
        :key="item.id"
      >
        <Award
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.templateTitle }}</strong>
          <p>{{ item.holder }} · {{ item.department }} · 颁发于 {{ item.issuedAt }}</p>
        </div>
        <span>{{ item.expiresAt }}</span
        ><b :data-status="item.status">{{ item.status }}</b>
      </article>
      <p
        v-if="issuedVisible().length === 0"
        :class="$style.empty"
      >
        没有匹配的已发证书
      </p>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4;
}
.cards {
  @apply grid grid-cols-3 gap-3;
}
.cards > article {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.certificate {
  @apply relative flex h-[274px] flex-col items-center px-5 py-4;
  text-align: center;
  color: #17305b;
  background: #edf4ff;
}
.cards > article[data-tone='cyan'] .certificate {
  background: #edfcff;
}
.cards > article[data-tone='green'] .certificate {
  color: #075f44;
  background: #edfff8;
}
.cards > article[data-tone='purple'] .certificate {
  color: #403276;
  background: #f3f0ff;
}
.cards > article[data-tone='orange'] .certificate {
  color: #7a4600;
  background: #fff8e8;
}
.certificate header {
  @apply flex w-full items-center gap-2;
  text-align: left;
}
.certificate header > span {
  @apply ml-auto rounded bg-white/70 px-2 py-1 text-[8px];
}
.certificate header strong {
  @apply text-[9px];
}
.certificate > p {
  @apply m-0 mt-6 text-[9px];
}
.certificate h2 {
  @apply m-0 mt-2 text-[14px];
}
.certificate > small {
  @apply mt-3 text-[8px];
}
.certificate > b {
  @apply mt-1 text-[18px];
  color: var(--app-primary);
}
.certificate dl {
  @apply mt-4 grid grid-cols-3 gap-5;
}
.certificate dt {
  @apply text-[7px];
  color: var(--ui-text-subtle);
}
.certificate dd {
  @apply m-0 mt-1 text-[8px] font-semibold;
}
.certificate > svg:last-child {
  @apply absolute bottom-4 right-4;
}
.cardBody {
  @apply p-3;
}
.cardBody h3 {
  @apply m-0 text-[11px];
}
.cardBody > p {
  @apply m-0 mt-1 flex justify-between text-[9px];
  color: var(--ui-text-subtle);
}
.cardBody > div {
  @apply mt-3 grid grid-cols-3 gap-2;
}
.cardBody > div span {
  @apply flex flex-col rounded p-2 text-[8px];
  background: var(--ui-surface-subtle);
}
.cardBody > div strong {
  @apply mt-1 text-[12px];
  color: var(--ui-text);
}
.cardBody button {
  @apply mt-3 w-full rounded border py-1.5 text-[9px];
  border-color: var(--ui-border);
  color: var(--app-primary);
  background: white;
}
.list {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.list article {
  @apply flex h-[68px] items-center gap-3 border-b px-4;
  border-color: var(--ui-border-subtle);
}
.list article > svg {
  color: var(--app-primary);
}
.list article > div {
  @apply flex flex-1 flex-col;
}
.list strong {
  @apply text-[10px];
}
.list p {
  @apply m-0 mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.list article > span {
  @apply w-28 text-[9px];
  color: var(--ui-text-muted);
}
.list article > b {
  @apply rounded px-2 py-1 text-[8px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.list article > b[data-status='即将到期'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.empty {
  @apply col-span-3 m-0 py-16 text-center text-[10px];
  color: var(--ui-text-subtle);
}
</style>
