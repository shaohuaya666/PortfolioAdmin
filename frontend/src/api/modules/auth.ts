import request from '../request'
import type { LoginRequest, LoginResponse, ChangePasswordRequest, CaptchaData, CaptchaVerifyRequest } from '@/types'

export const authApi = {
  login(data: LoginRequest) {
    return request.post<LoginResponse>('/Auth/login', data)
  },
  changePassword(data: ChangePasswordRequest) {
    return request.post('/Auth/change-password', data)
  }
}

export const captchaApi = {
  generate() {
    return request.get<CaptchaData>('/Captcha/generate')
  },
  verify(data: CaptchaVerifyRequest) {
    return request.post('/Captcha/verify', data)
  }
}
