<script setup lang="ts">
import { Images, Upload } from 'lucide-vue-next';
import { ref } from 'vue';
import type { Product } from './product.types';
import { seedMediaAssets } from './product.media.data';
import type { MediaAsset } from './product.media.types';
import ProductMediaLibrary from './ProductMediaLibrary.vue';
import ProductMediaUpload from './ProductMediaUpload.vue';

defineProps<{ products: Product[] }>();

const activeView = ref<'upload' | 'library'>('upload');
const assets = ref<MediaAsset[]>(structuredClone(seedMediaAssets));

function addAssets(incoming: MediaAsset[]) {
  assets.value = [...assets.value, ...incoming];
}

function replaceAsset(assetId: string, replacement: MediaAsset) {
  assets.value = assets.value.map((asset) => (asset.id === assetId ? { ...replacement, id: assetId } : asset));
}

function deleteAssets(ids: Set<string>) {
  assets.value = assets.value.filter((asset) => !ids.has(asset.id));
}
</script>

<template>
  <section :class="$style.section">
    <div :class="$style.row">
      <button
        :class="[$style.button, activeView === 'upload' ? $style.stateSelected : $style.stateInverted]"
        @click="activeView = 'upload'"
      >
        <Upload :size="14" /><span
          >批量上传与匹配<small :class="$style.caption">拖拽上传 · 自动识别物料编码 / 型号</small></span
        ><i
          v-if="activeView === 'upload'"
          :class="$style.decoration"
        />
      </button>
      <button
        :class="[$style.button, activeView === 'library' ? $style.stateSelected : $style.stateInverted]"
        @click="activeView = 'library'"
      >
        <Images :size="14" /><span
          >商品图片文档台账<small :class="$style.caption"
            >所有物料 · 共 {{ assets.length }} 个资产 · 支持批量维护与更换</small
          ></span
        ><i
          v-if="activeView === 'library'"
          :class="$style.decoration"
        />
      </button>
    </div>
    <ProductMediaUpload
      v-if="activeView === 'upload'"
      :products="products"
      :assets="assets"
      @commit="addAssets"
    />
    <ProductMediaLibrary
      v-else
      :products="products"
      :assets="assets"
      @add="addAssets"
      @replace="replaceAsset"
      @delete="deleteAssets"
    />
  </section>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.section {
  @apply mt-4 overflow-hidden rounded-lg border border-[#DFE1E6] bg-white;
}
.row {
  @apply flex border-b border-[#DFE1E6] bg-[#FAFBFC];
}
.button {
  @apply relative flex flex-1 items-center gap-2 px-5 py-3 text-left text-[13px] transition-colors;
}
.stateSelected {
  @apply bg-white font-semibold text-app-primary;
}
.stateInverted {
  @apply text-[#5E6C84] hover:bg-white/60;
}
.caption {
  @apply mt-0.5 block text-[10px] font-normal text-[#97A0AF];
}
.decoration {
  @apply absolute inset-x-0 bottom-0 h-0.5 bg-app-primary;
}
</style>
