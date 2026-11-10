<script setup lang="ts">
import { Share2 } from 'lucide-vue-next';
import { drawingIssueLogReferenceFixture } from '../model/drawing-issue-log.data';
import DrawingRegisterPage from './register/DrawingRegisterPage.vue';
</script>

<template>
  <DrawingRegisterPage
    :fixture="drawingIssueLogReferenceFixture"
    :icon="Share2"
    implementation="drawing-issue-log"
    route="/drawings/drawing-issue-log"
  />
</template>

<style module>
@reference "../../../styles/index.css";
</style>
