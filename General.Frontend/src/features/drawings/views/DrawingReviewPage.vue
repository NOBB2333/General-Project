<script setup lang="ts">
import { ClipboardCheck } from 'lucide-vue-next';
import { drawingReviewReferenceFixture } from '../model/drawing-review.data';
import DrawingRegisterPage from './register/DrawingRegisterPage.vue';
</script>

<template>
  <DrawingRegisterPage
    :fixture="drawingReviewReferenceFixture"
    :icon="ClipboardCheck"
    implementation="drawing-review"
    route="/drawings/drawing-review"
  />
</template>

<style module>
@reference "../../../styles/index.css";
</style>
