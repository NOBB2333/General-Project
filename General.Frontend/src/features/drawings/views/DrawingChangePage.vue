<script setup lang="ts">
import { GitBranch } from 'lucide-vue-next';
import { drawingChangeReferenceFixture } from '../model/drawing-change.data';
import DrawingRegisterPage from './register/DrawingRegisterPage.vue';
</script>

<template>
  <DrawingRegisterPage
    :fixture="drawingChangeReferenceFixture"
    :icon="GitBranch"
    implementation="drawing-change"
    route="/drawings/drawing-change"
  />
</template>

<style module>
@reference "../../../styles/index.css";
</style>
