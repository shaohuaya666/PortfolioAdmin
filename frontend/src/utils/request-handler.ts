/**
 * 统一请求处理工具
 * 用于包装 API 请求，提供统一的错误处理
 */

import { ElMessage } from 'element-plus'

/**
 * 安全地执行异步请求
 * @param requestFn 请求函数
 * @param options 配置选项
 * @returns Promise<T | null>
 */
export async function safeRequest<T>(
  requestFn: () => Promise<T>,
  options: {
    silent?: boolean // 静默模式，不显示错误提示
    onError?: (error: any) => void // 自定义错误处理
    defaultValue?: T // 错误时返回的默认值
  } = {}
): Promise<T | null> {
  try {
    return await requestFn()
  } catch (error: any) {
    // 如果是 401 错误（token 过期），不需要额外处理，拦截器已经处理了
    if (error.message === 'Unauthorized') {
      return options.defaultValue !== undefined ? options.defaultValue : null
    }

    // 执行自定义错误处理
    if (options.onError) {
      options.onError(error)
    }

    // 非静默模式且没有自定义错误处理时，显示错误消息
    if (!options.silent && !options.onError) {
      const message = error.response?.data?.message || error.message || '操作失败'
      ElMessage.error(message)
    }

    return options.defaultValue !== undefined ? options.defaultValue : null
  }
}

/**
 * 批量请求处理（适用于页面初始化时需要加载多个数据源的场景）
 * @param requests 请求配置数组
 * @returns Promise<any[]>
 */
export async function batchRequest(
  requests: Array<{
    name: string
    request: () => Promise<any>
    required?: boolean // 是否必需，为 true 时失败会中断后续请求
  }>
): Promise<Record<string, any>> {
  const results: Record<string, any> = {}

  for (const { name, request, required = false } of requests) {
    try {
      results[name] = await request()
    } catch (error: any) {
      // 如果是 token 过期，直接返回，拦截器会处理跳转
      if (error.message === 'Unauthorized') {
        return results
      }

      results[name] = null

      // 必需的请求失败时，中断后续请求
      if (required) {
        console.error(`Required request '${name}' failed:`, error)
        break
      }
    }
  }

  return results
}
