import request from '../request'
import type { LoginRequest, LoginResponse } from '@/types'

export const authApi = {
  login(data: LoginRequest) {
    return request.post<LoginResponse>('/Auth/login', data)
  }
}
