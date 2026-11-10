<script setup lang="ts">
import { BarChart3, Clock3, Radar, Timer, TrendingUp } from 'lucide-vue-next';
import type { AttendanceStatsFixture } from '../../model/attendance-stats.types';
defineProps<{ fixture: AttendanceStatsFixture }>();
</script>

<template>
  <section :class="$style.grid">
    <article :class="$style.trendPanel">
      <h2><TrendingUp :size="17" />每日考勤趋势 (3月)</h2>
      <div :class="$style.chart">
        <div :class="$style.yAxis"><span>12</span><span>9</span><span>6</span><span>3</span><span>0</span></div>
        <div :class="$style.bars">
          <div
            v-for="item in fixture.trend"
            :key="item.date"
            :class="$style.day"
          >
            <div :class="$style.stack">
              <i
                :data-value="item.normal"
                data-tone="normal"
              /><i
                :data-value="item.late"
                data-tone="late"
              /><i
                :data-value="item.early"
                data-tone="early"
              /><i
                :data-value="item.absent"
                data-tone="absent"
              /><i
                :data-value="item.leave"
                data-tone="leave"
              />
            </div>
            <span>{{ item.date }}</span>
          </div>
        </div>
      </div>
      <div :class="$style.legend">
        <span data-tone="normal">正常</span><span data-tone="late">迟到</span><span data-tone="early">早退</span
        ><span data-tone="absent">旷工</span><span data-tone="leave">请假</span>
      </div>
    </article>
    <article :class="$style.statusPanel">
      <h2><Clock3 :size="17" />考勤状态分布</h2>
      <div :class="$style.donut">
        <span>86<small>总记录</small></span>
      </div>
      <div :class="$style.statusLegend">
        <span
          v-for="item in fixture.status"
          :key="item.label"
          ><i :data-tone="item.tone" />{{ item.label }} {{ item.value }}</span
        >
      </div>
    </article>
    <article :class="$style.rankingPanel">
      <h2><BarChart3 :size="17" />部门出勤率排名</h2>
      <ol>
        <li
          v-for="item in fixture.departments"
          :key="item.name"
        >
          <b>{{ item.rank }}</b
          ><span>{{ item.name }}</span>
          <div><i :data-rate="item.rate" /></div>
          <strong>{{ item.rate }}</strong>
        </li>
      </ol>
    </article>
    <article :class="$style.latePanel">
      <h2><Timer :size="17" />迟到时长分布</h2>
      <div :class="$style.lateBars">
        <span
          v-for="item in fixture.lateDistribution"
          :key="item.label"
          ><i :data-value="item.value" /><b>{{ item.value }}</b
          ><small>{{ item.label }}</small></span
        >
      </div>
      <p>迟到占比 <strong>15%</strong>，建议关注</p>
    </article>
    <article :class="$style.radarPanel">
      <h2><Radar :size="17" />部门考勤雷达图</h2>
      <div :class="$style.radar">
        <i /><b>产品部</b><b>技术部</b><b>市场部</b><b>销售部</b><b>财务部</b><b>人事部</b>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-12 gap-3;
}
.grid > article {
  @apply min-h-[210px] overflow-hidden rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid h2 {
  @apply flex items-center gap-2 text-[14px] font-semibold;
  color: #172b4d;
}
.grid h2 svg {
  color: #0052cc;
}
.trendPanel {
  @apply col-span-8 min-h-[340px];
}
.statusPanel {
  @apply col-span-4 min-h-[340px];
}
.rankingPanel {
  @apply col-span-4;
}
.latePanel {
  @apply col-span-4;
}
.radarPanel {
  @apply col-span-4;
}
.chart {
  @apply mt-3 flex h-[220px] border-b border-l;
  border-color: #dfe1e6;
  background: repeating-linear-gradient(to bottom, transparent 0 53px, #ebecf0 54px);
}
.yAxis {
  @apply flex w-9 flex-col justify-between pb-0 pt-0 text-right text-[10px];
  color: #6b778c;
}
.bars {
  @apply flex flex-1 items-end justify-around px-4;
}
.day {
  @apply flex h-full flex-1 flex-col items-center justify-end gap-2;
}
.stack {
  @apply flex w-[70%] flex-col-reverse justify-start;
}
.stack i {
  @apply block w-full;
}
.stack i[data-value='0'] {
  height: 0;
}
.stack i[data-value='1'] {
  height: 18px;
}
.stack i[data-value='2'] {
  height: 36px;
}
.stack i[data-value='3'] {
  height: 54px;
}
.stack i[data-value='4'] {
  height: 72px;
}
.stack i[data-value='5'] {
  height: 90px;
}
.stack i[data-value='6'] {
  height: 108px;
}
.stack i[data-value='7'] {
  height: 126px;
}
.stack i[data-value='8'] {
  height: 144px;
}
.stack i[data-tone='normal'] {
  background: #00875a;
}
.stack i[data-tone='late'] {
  background: #ff8b00;
}
.stack i[data-tone='early'] {
  background: #ff5630;
}
.stack i[data-tone='absent'] {
  background: #c9371c;
}
.stack i[data-tone='leave'] {
  background: #6554c0;
}
.day > span {
  @apply text-[10px];
  color: #6b778c;
}
.legend {
  @apply mt-3 flex justify-center gap-4 text-[10px];
}
.legend span::before {
  content: '';
  @apply mr-1 inline-block h-2.5 w-2.5;
}
.legend span[data-tone='normal'] {
  color: #00875a;
}
.legend span[data-tone='normal']::before {
  background: #00875a;
}
.legend span[data-tone='late'] {
  color: #ff8b00;
}
.legend span[data-tone='late']::before {
  background: #ff8b00;
}
.legend span[data-tone='early'] {
  color: #ff5630;
}
.legend span[data-tone='early']::before {
  background: #ff5630;
}
.legend span[data-tone='absent'] {
  color: #c9371c;
}
.legend span[data-tone='absent']::before {
  background: #c9371c;
}
.legend span[data-tone='leave'] {
  color: #6554c0;
}
.legend span[data-tone='leave']::before {
  background: #6554c0;
}
.donut {
  @apply hidden;
  background: conic-gradient(#00875a 0 57%, #ff8b00 57% 72%, #ff5630 72% 82%, #c9371c 82% 88%, #6554c0 88% 100%);
}
.donut::before {
  @apply absolute h-[78px] w-[78px] rounded-full bg-white;
  content: '';
}
.donut span {
  @apply relative z-10 text-center text-[22px] font-bold;
  color: #172b4d;
}
.donut small {
  @apply block text-[9px] font-normal;
  color: #6b778c;
}
.statusLegend {
  @apply mt-[240px] flex flex-wrap justify-center gap-x-3 gap-y-2 text-[10px];
  color: #6b778c;
}
.statusLegend i {
  @apply mr-1 inline-block h-2 w-2 rounded-full;
}
.statusLegend i[data-tone='normal'] {
  background: #00875a;
}
.statusLegend i[data-tone='late'] {
  background: #ff8b00;
}
.statusLegend i[data-tone='early'] {
  background: #ff5630;
}
.statusLegend i[data-tone='absent'] {
  background: #c9371c;
}
.statusLegend i[data-tone='leave'] {
  background: #6554c0;
}
.statusLegend i[data-tone='travel'] {
  background: #0052cc;
}
.rankingPanel ol {
  @apply mt-4 space-y-2;
}
.rankingPanel li {
  @apply flex items-center gap-2 text-[10px];
}
.rankingPanel li > b {
  @apply flex h-5 w-5 items-center justify-center rounded-full bg-[#FFAB00] text-white;
}
.rankingPanel li:nth-child(n + 4) > b {
  background: #deebff;
  color: #0052cc;
}
.rankingPanel li > span {
  @apply w-12;
}
.rankingPanel li > div {
  @apply h-2 flex-1 overflow-hidden rounded bg-[#EBECF0];
}
.rankingPanel li > div i {
  @apply block h-full rounded bg-[#36B37E];
}
.rankingPanel li > div i[data-rate='75%'] {
  width: 75%;
}
.rankingPanel li > div i[data-rate='72%'] {
  width: 72%;
}
.rankingPanel li > div i[data-rate='69.2%'] {
  width: 69.2%;
}
.rankingPanel li > div i[data-rate='66.7%'] {
  width: 66.7%;
}
.rankingPanel li > div i[data-rate='64.7%'] {
  width: 64.7%;
}
.rankingPanel li > div i[data-rate='62.5%'] {
  width: 62.5%;
}
.rankingPanel li strong {
  @apply w-10 text-right text-[11px];
  color: #00875a;
}
.lateBars {
  @apply mt-5 flex h-[125px] items-end justify-around border-b border-l px-2;
  border-color: #dfe1e6;
}
.lateBars > span {
  @apply flex h-full flex-1 flex-col items-center justify-end;
}
.lateBars i {
  @apply w-7 rounded-t bg-[#FFAB00];
}
.lateBars i[data-value='2'] {
  height: 24px;
}
.lateBars i[data-value='3'] {
  height: 36px;
}
.lateBars i[data-value='5'] {
  height: 60px;
}
.lateBars i[data-value='6'] {
  height: 72px;
}
.lateBars i[data-value='8'] {
  height: 96px;
}
.lateBars b {
  @apply text-[10px] text-[#42526E];
}
.lateBars small {
  @apply mt-2 whitespace-nowrap text-[9px] text-[#6B778C];
}
.latePanel p {
  @apply mt-4 text-[11px] text-[#6B778C];
}
.latePanel p strong {
  color: #de350b;
}
.radar {
  @apply relative mx-auto mt-2 h-[165px] w-[190px];
}
.radar::before {
  content: '';
  @apply absolute left-1/2 top-1/2 h-[122px] w-[122px] -translate-x-1/2 -translate-y-1/2 rotate-45 border border-[#B3D4FF];
  background: rgba(222, 235, 255, 0.45);
  clip-path: polygon(50% 0, 100% 35%, 82% 100%, 18% 100%, 0 35%);
}
.radar i {
  @apply absolute left-1/2 top-1/2 z-10 h-[92px] w-[92px] -translate-x-1/2 -translate-y-1/2 rotate-45 border-2 border-[#0052CC];
  background: rgba(0, 82, 204, 0.18);
  clip-path: polygon(50% 0, 100% 35%, 82% 100%, 18% 100%, 0 35%);
}
.radar b {
  @apply absolute text-[9px] font-normal text-[#6B778C];
}
.radar b:nth-of-type(1) {
  top: 0;
  left: 72px;
}
.radar b:nth-of-type(2) {
  right: 0;
  top: 48px;
}
.radar b:nth-of-type(3) {
  right: 3px;
  bottom: 18px;
}
.radar b:nth-of-type(4) {
  left: 59px;
  bottom: 0;
}
.radar b:nth-of-type(5) {
  left: 0;
  bottom: 18px;
}
.radar b:nth-of-type(6) {
  left: 0;
  top: 48px;
}
</style>
