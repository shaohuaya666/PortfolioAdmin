import request from '../request'
import type { LoginRequest, LoginResponse, ChangePasswordRequest } from '@/types'

export const authApi = {
  login(data: LoginRequest) {
    return request.post<LoginResponse>('/Auth/login', data)
  },
  changePassword(data: ChangePasswordRequest) {
    return request.post('/Auth/change-password', data)
  }
}
