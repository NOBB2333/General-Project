import assert from 'node:assert/strict';
import test from 'node:test';

import { adaptBackendMenuTree } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

const businessMenu = [
  {
    component: 'BasicLayout',
    name: 'BusinessCenter',
    path: '/business',
    children: [
      {
        component: '/business/overview/index',
        name: 'BusinessOverview',
        path: '/business/overview',
        meta: { title: '经营总览' },
      },
      {
        component: '/business/projects/index',
        name: 'BusinessProjects',
        path: '/business/projects',
        meta: { title: '项目经营' },
      },
      {
        component: '/business/reports/index',
        name: 'BusinessReports',
        path: '/business/reports',
        meta: { title: '报表中心' },
      },
    ],
  },
];

test('business menu entries resolve only through the registered Chao routes', () => {
  const nodes = adaptBackendMenuTree(businessMenu, ['Business.Report.View']);

  assert.equal(nodes.length, 1);
  assert.equal(nodes[0]?.kind, 'group');
  assert.deepEqual(
    nodes[0]?.children.map((node) => [node.routeName, node.localPath]),
    [
      [routeNames.businessOverview, '/business/overview'],
      [routeNames.businessProjects, '/business/projects'],
      [routeNames.businessReports, '/business/reports'],
    ],
  );
});

test('business reports and overview stay hidden without Business.Report.View', () => {
  const nodes = adaptBackendMenuTree(businessMenu, []);

  assert.equal(nodes.length, 1);
  assert.deepEqual(
    nodes[0]?.children.map((node) => node.routeName),
    [routeNames.businessProjects],
  );
});
