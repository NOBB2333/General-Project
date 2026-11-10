<script setup lang="ts">
import { Handshake } from 'lucide-vue-next';
import { customerDrawingsReferenceFixture } from '../model/customer-drawings.data';
import DrawingRegisterPage from './register/DrawingRegisterPage.vue';
</script>

<template>
  <DrawingRegisterPage
    :fixture="customerDrawingsReferenceFixture"
    :icon="Handshake"
    implementation="customer-drawings"
    route="/drawings/customer-drawings"
  />
</template>

<style module>
@reference "../../../styles/index.css";
</style>
