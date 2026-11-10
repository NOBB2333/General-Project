<script setup lang="ts">
import QuoteEditorDialog from './sales-quotes/QuoteEditorDialog.vue';
import QuoteOverview from './sales-quotes/QuoteOverview.vue';
import QuoteScopeBar from './sales-quotes/QuoteScopeBar.vue';
import QuotesAnalysis from './sales-quotes/QuotesAnalysis.vue';
import QuotesHero from './sales-quotes/QuotesHero.vue';
import QuotesWorkspace from './sales-quotes/QuotesWorkspace.vue';
import { useSalesQuotesPage } from './sales-quotes/useSalesQuotesPage';

const page = useSalesQuotesPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/sales-quotes"
    data-page-implementation="crm-sales-quotes"
  >
    <QuotesHero />
    <QuoteScopeBar
      v-model:scope="page.scope.value"
      v-model:view="page.view.value"
      :counts="page.scopeCounts.value"
    />
    <QuoteOverview
      :total-amount="page.totalAmount.value"
      :won-amount="page.wonAmount.value"
      :follow-up-count="page.followUpCount.value"
      :win-rate="page.winRate.value"
      :total="page.rows.value.length"
      :won-count="page.wonCount.value"
    />
    <QuotesWorkspace
      v-if="page.view.value === 'list'"
      v-model:query="page.query.value"
      v-model:status="page.status.value"
      :rows="page.filteredRows.value"
      :counts="page.statusCounts.value"
      @create="page.editorOpen.value = true"
      @export="page.exportQuotes"
    />
    <QuotesAnalysis
      v-else
      :rows="page.scopedRows.value"
    />
  </main>

  <QuoteEditorDialog
    v-model:open="page.editorOpen.value"
    :customers="page.customers"
    :owners="page.owners"
    @save="page.saveQuote"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
}
</style>
