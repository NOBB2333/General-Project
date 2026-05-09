import type { BasicUserInfo } from '@vben-core/typings';

/** 用户信息 */
interface UserInfo extends BasicUserInfo {
  /**
   * 用户描述
   */
  desc: string;
  /**
   * 首页地址
   */
  homePath: string;

  /**
   * 是否处于 Host 运维租户模式
   */
  isHostTenantOperation?: boolean;

  /**
   * 当前运维租户
   */
  operationTenantId?: null | string;
  operationTenantName?: null | string;

  /**
   * accessToken
   */
  token: string;
}

export type { UserInfo };
