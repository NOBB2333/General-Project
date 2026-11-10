<script setup lang="ts">
import { X } from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { UiSelect } from '@/components/ui';
import type { Product, ProductAlias } from './product.types';

const props = defineProps<{ open: boolean; alias: ProductAlias | null; products: Product[]; customers: string[] }>();
const emit = defineEmits<{ close: []; save: [alias: ProductAlias] }>();

const form = reactive<ProductAlias>({
  id: '',
  customer: '',
  customerAbbr: '',
  product: '',
  model: '',
  alias: '',
  aliasModel: '',
  notes: '',
});
const error = ref('');
const productOptions = computed(() => {
  const current = props.products.map((product) => ({ name: product.name, model: product.model }));
  if (props.alias && !current.some((item) => item.name === props.alias?.product))
    current.unshift({ name: props.alias.product, model: props.alias.model });
  return current;
});
const customerSelectOptions = computed(() => [
  { value: '', label: '请选择客户' },
  ...props.customers.map((value) => ({ value, label: value })),
]);
const productSelectOptions = computed(() => [
  { value: '', label: '请选择商品' },
  ...productOptions.value.map((item) => ({ value: item.name, label: `${item.name} · ${item.model}` })),
]);

function selectProduct(value: string) {
  form.product = value;
  form.model = productOptions.value.find((item) => item.name === form.product)?.model ?? '';
}

function save() {
  if (!form.customer || !form.product || !form.alias.trim() || !form.aliasModel.trim()) {
    error.value = '请完整填写必填字段';
    return;
  }
  emit('save', { ...form, id: form.id || `CPA-${Date.now()}`, customerAbbr: form.customer.slice(0, 1) });
}

function onKeydown(event: KeyboardEvent) {
  if (props.open && event.key === 'Escape') emit('close');
}

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(
      form,
      props.alias ?? {
        id: '',
        customer: '',
        customerAbbr: '',
        product: '',
        model: '',
        alias: '',
        aliasModel: '',
        notes: '',
      },
    );
    error.value = '';
  },
);
onMounted(() => document.addEventListener('keydown', onKeydown));
onUnmounted(() => document.removeEventListener('keydown', onKeydown));
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      @mousedown.self="emit('close')"
      :class="$style.overlay"
    >
      <section
        role="dialog"
        aria-modal="true"
        :aria-label="alias ? '编辑别名' : '新增别名'"
        :class="$style.dialog"
      >
        <header :class="$style.header">
          <div>
            <h2 :class="$style.title">{{ alias ? '编辑商品别名' : '新增商品别名' }}</h2>
            <p :class="$style.description">为指定客户维护其内部使用的商品名称和型号</p>
          </div>
          <button
            title="关闭"
            @click="emit('close')"
            :class="$style.closeButton"
          >
            <X :size="16" />
          </button>
        </header>
        <div :class="$style.list">
          <div :class="$style.grid">
            <label :class="$style.field"
              ><span :class="$style.label">客户名称 <b :class="$style.b">*</b></span
              ><UiSelect
                v-model="form.customer"
                :options="customerSelectOptions"
                label="选择客户"
                :class="$style.select" /></label
            ><label :class="$style.field"
              ><span :class="$style.label">关联商品 <b :class="$style.b">*</b></span
              ><UiSelect
                :model-value="form.product"
                :options="productSelectOptions"
                label="选择关联商品"
                :class="$style.select"
                @update:model-value="selectProduct"
            /></label>
          </div>
          <div :class="$style.card">
            <span :class="$style.labelCompact">系统商品型号</span
            ><strong :class="$style.strongLabel">{{ form.model || '选择商品后自动带出' }}</strong>
          </div>
          <div :class="$style.grid">
            <label :class="$style.field"
              ><span :class="$style.label">客户商品名称 <b :class="$style.b">*</b></span
              ><input
                v-model="form.alias"
                placeholder="请输入客户使用的商品名称"
                :class="$style.input" /></label
            ><label :class="$style.field"
              ><span :class="$style.label">客户商品型号 <b :class="$style.b">*</b></span
              ><input
                v-model="form.aliasModel"
                placeholder="请输入客户内部型号"
                :class="$style.input"
            /></label>
          </div>
          <label :class="$style.field"
            ><span :class="$style.label">备注</span
            ><textarea
              v-model="form.notes"
              rows="3"
              placeholder="可记录项目、应用场景或客户特殊要求"
              :class="$style.textarea"
            />
          </label>
          <p
            v-if="error"
            :class="$style.descriptionDanger"
          >
            {{ error }}
          </p>
        </div>
        <footer :class="$style.footer">
          <button
            @click="emit('close')"
            :class="$style.button"
          >
            取消</button
          ><button
            @click="save"
            :class="$style.primaryButton"
          >
            {{ alias ? '保存修改' : '确认添加' }}
          </button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.overlay {
  @apply fixed inset-0 z-[150] flex items-center justify-center bg-black/40;
}
.dialog {
  @apply w-[620px] overflow-hidden rounded-xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply flex items-center border-b border-[#DFE1E6] px-6 py-4;
}
.title {
  @apply text-[16px] font-semibold;
}
.description {
  @apply mt-0.5 text-[11px] text-[#6B778C];
}
.closeButton {
  @apply ml-auto rounded p-1.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.list {
  @apply space-y-4 p-6;
}
.grid {
  @apply grid grid-cols-2 gap-4;
}
.field {
  @apply block;
}
.label {
  @apply mb-1.5 block text-[12px] font-medium;
}
.b {
  @apply text-[#DE350B];
}
.select {
  @apply h-9 w-full rounded border border-[#DFE1E6] bg-white px-3 text-[12px] outline-none focus:border-[#4C9AFF];
}
.card {
  @apply rounded-lg border border-[#DFE1E6] bg-[#FAFBFC] p-3;
}
.labelCompact {
  @apply text-[10px] text-[#6B778C];
}
.strongLabel {
  @apply mt-1 block text-[12px];
}
.input {
  @apply h-9 w-full rounded border border-[#DFE1E6] px-3 text-[12px] outline-none focus:border-[#4C9AFF];
}
.textarea {
  @apply w-full resize-none rounded border border-[#DFE1E6] px-3 py-2 text-[12px] outline-none focus:border-[#4C9AFF];
}
.descriptionDanger {
  @apply rounded bg-[#FFEBE6] px-3 py-2 text-[11px] text-[#DE350B];
}
.footer {
  @apply flex justify-end gap-2 border-t border-[#DFE1E6] bg-[#FAFBFC] px-6 py-4;
}
.button {
  @apply rounded border border-[#DFE1E6] bg-white px-4 py-2 text-[12px] text-[#42526E];
}
.primaryButton {
  @apply rounded bg-app-primary px-5 py-2 text-[12px] font-medium text-white hover:bg-app-primary-hover;
}
</style>
