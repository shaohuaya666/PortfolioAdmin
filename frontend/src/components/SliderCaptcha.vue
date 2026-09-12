<template>
  <div class="slider-captcha" :class="theme" :style="{ width: captchaData.bgWidth + 'px' }">
    <!-- 背景图 + 拼图块浮层 -->
    <div
      class="captcha-canvas-wrapper"
      :style="{ width: captchaData.bgWidth + 'px', height: captchaData.bgHeight + 'px' }"
    >
      <canvas ref="bgCanvasRef"></canvas>
      <canvas
        ref="puzzleCanvasRef"
        class="puzzle-overlay"
        :style="{
          left: puzzleLeft + 'px',
          top: captchaData.targetY + 'px',
          width: captchaData.puzzleWidth + 'px',
          height: captchaData.puzzleHeight + 'px'
        }"
      ></canvas>

      <!-- 状态遮罩 -->
      <div v-if="status === 'failed'" class="captcha-mask captcha-error" @click="refresh">
        <span>验证失败，点击刷新</span>
      </div>
      <div v-if="status === 'success'" class="captcha-mask captcha-ok">
        <span>✓ 验证通过</span>
      </div>
    </div>

    <!-- 滑块轨道 -->
    <div
      class="slider-track"
      ref="trackRef"
      :style="{ width: captchaData.bgWidth + 'px' }"
      @mousedown.prevent="onDragStart"
      @touchstart.prevent="onDragStart"
    >
      <div class="slider-progress" :style="{ width: sliderLeft + captchaData.puzzleWidth / 2 + 'px' }"></div>
      <div
        class="slider-btn"
        :class="{ sliding: status === 'sliding', verified: status === 'success' }"
        :style="{ left: sliderBtnLeft + 'px', width: captchaData.puzzleWidth + 'px' }"
      >
        <span class="slider-icon">
          <template v-if="status === 'success'">✓</template>
          <template v-else>⟫</template>
        </span>
      </div>
      <span class="slider-hint" v-if="status === 'idle'">向右滑动完成验证</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, nextTick } from 'vue'
import { captchaApi } from '@/api/modules/auth'
import type { CaptchaData, CaptchaTrackPoint } from '@/types'

const props = withDefaults(defineProps<{
  theme?: 'light' | 'dark'
}>(), {
  theme: 'dark'
})

const emit = defineEmits<{
  verified: [captchaId: string]
}>()

// ===== 主题颜色 =====
interface ThemeColors {
  bgFill: string
  textureColor: (rng: () => number, minA: number, maxA: number) => string
  gapFill: string
  gapStroke: string
  puzzleStroke: string
  puzzleShadowFilter: string
}

const lightColors: ThemeColors = {
  bgFill: '#eef2f7',
  textureColor: (rng, minA, maxA) => {
    const a = minA + rng() * (maxA - minA)
    const gray = 30 + rng() * 120
    return `rgba(${gray},${gray},${gray},${a})`
  },
  gapFill: 'rgba(0, 0, 0, 0.25)',
  gapStroke: 'rgba(0, 0, 0, 0.35)',
  puzzleStroke: '#06b6d4',
  puzzleShadowFilter: 'drop-shadow(2px 2px 4px rgba(0,0,0,0.35))'
}

const darkColors: ThemeColors = {
  bgFill: '#0a1628',
  textureColor: (rng, minA, maxA) => {
    const a = minA + rng() * (maxA - minA)
    return `rgba(255,255,255,${a})`
  },
  gapFill: 'rgba(6, 182, 212, 0.15)',
  gapStroke: 'rgba(6, 182, 212, 0.3)',
  puzzleStroke: '#06b6d4',
  puzzleShadowFilter: 'drop-shadow(2px 2px 6px rgba(6,182,212,0.3))'
}

function getColors(): ThemeColors {
  return props.theme === 'dark' ? darkColors : lightColors
}

// ===== 状态 =====
type Status = 'idle' | 'sliding' | 'verifying' | 'success' | 'failed'
const status = ref<Status>('idle')

const captchaData = reactive<CaptchaData>({
  captchaId: '',
  puzzleWidth: 50,
  puzzleHeight: 50,
  bgWidth: 300,
  bgHeight: 145,
  targetX: 0,
  targetY: 50
})

const bgCanvasRef = ref<HTMLCanvasElement>()
const puzzleCanvasRef = ref<HTMLCanvasElement>()
const trackRef = ref<HTMLDivElement>()

const puzzleLeft = ref(0)
const sliderBtnLeft = ref(0)
const sliderLeft = ref(0)

const trackData = ref<CaptchaTrackPoint[]>([])
let dragStartX = 0
let dragStartLeft = 0
let dragging = false
let trackStartTime = 0
let puzzleSeed = 0

// ===== 生命周期 =====
onMounted(async () => {
  await fetchCaptcha()
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
  document.addEventListener('touchmove', onTouchMove, { passive: false })
  document.addEventListener('touchend', onTouchEnd)
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
  document.removeEventListener('touchmove', onTouchMove)
  document.removeEventListener('touchend', onTouchEnd)
})

async function fetchCaptcha() {
  status.value = 'idle'
  puzzleLeft.value = 0
  sliderBtnLeft.value = 0
  sliderLeft.value = 0
  trackData.value = []
  puzzleSeed = Math.floor(Math.random() * 100000)

  try {
    const res = await captchaApi.generate()
    Object.assign(captchaData, res.data)
    await nextTick()
    drawCanvases()
  } catch {
    setTimeout(fetchCaptcha, 3000)
  }
}

// ===== Canvas 绘制 =====
function drawCanvases() {
  drawBackground()
  drawPuzzle()
}

function drawBackground() {
  const canvas = bgCanvasRef.value
  if (!canvas) return
  const { bgWidth, bgHeight, puzzleWidth, puzzleHeight, targetX, targetY } = captchaData
  const dpr = window.devicePixelRatio || 1
  canvas.width = bgWidth * dpr
  canvas.height = bgHeight * dpr
  canvas.style.width = bgWidth + 'px'
  canvas.style.height = bgHeight + 'px'

  const ctx = canvas.getContext('2d')!
  ctx.scale(dpr, dpr)
  const c = getColors()

  ctx.fillStyle = c.bgFill
  ctx.fillRect(0, 0, bgWidth, bgHeight)

  drawBgTexture(ctx, bgWidth, bgHeight)

  // 缺口
  const gapPath = createPuzzlePath(targetX, targetY, puzzleWidth, puzzleHeight)
  ctx.save()
  ctx.clip(gapPath)
  ctx.fillStyle = c.gapFill
  ctx.fillRect(targetX - 2, targetY - 2, puzzleWidth + 4, puzzleHeight + 4)
  ctx.restore()

  ctx.save()
  ctx.strokeStyle = c.gapStroke
  ctx.lineWidth = 1.5
  ctx.setLineDash([4, 4])
  ctx.stroke(gapPath)
  ctx.restore()
}

function drawPuzzle() {
  const canvas = puzzleCanvasRef.value
  if (!canvas) return
  const { puzzleWidth, puzzleHeight, targetX, targetY, bgWidth, bgHeight } = captchaData
  const dpr = window.devicePixelRatio || 1
  canvas.width = puzzleWidth * dpr
  canvas.height = puzzleHeight * dpr
  canvas.style.width = puzzleWidth + 'px'
  canvas.style.height = puzzleHeight + 'px'

  const ctx = canvas.getContext('2d')!
  ctx.scale(dpr, dpr)
  ctx.clearRect(0, 0, puzzleWidth, puzzleHeight)
  const c = getColors()

  ctx.save()
  const path = createPuzzlePath(0, 0, puzzleWidth, puzzleHeight)
  ctx.clip(path)

  ctx.fillStyle = c.bgFill
  ctx.fillRect(0, 0, puzzleWidth, puzzleHeight)

  ctx.save()
  ctx.translate(-targetX, -targetY)
  drawBgTexture(ctx, bgWidth, bgHeight)
  ctx.restore()

  ctx.strokeStyle = c.puzzleStroke
  ctx.lineWidth = 2
  ctx.stroke(path)
  ctx.restore()

  canvas.style.filter = c.puzzleShadowFilter
}

function drawBgTexture(ctx: CanvasRenderingContext2D, w: number, h: number) {
  const rng = seedRandom(puzzleSeed)
  const c = getColors()

  const lineCount = 6 + Math.floor(rng() * 8)
  for (let i = 0; i < lineCount; i++) {
    const y = rng() * h
    ctx.strokeStyle = c.textureColor(rng, 0.03, 0.07)
    ctx.lineWidth = 1 + Math.floor(rng() * 2)
    ctx.beginPath()
    ctx.moveTo(0, y)
    if (rng() > 0.5) {
      ctx.setLineDash([8 + rng() * 14, 8 + rng() * 12])
    } else {
      ctx.setLineDash([])
    }
    ctx.lineTo(w, y)
    ctx.stroke()
  }
  ctx.setLineDash([])

  const dotCount = 12 + Math.floor(rng() * 16)
  for (let i = 0; i < dotCount; i++) {
    const x = rng() * w
    const y = rng() * h
    const r = 1 + rng() * 2.5
    ctx.fillStyle = c.textureColor(rng, 0.03, 0.08)
    ctx.beginPath()
    ctx.arc(x, y, r, 0, Math.PI * 2)
    ctx.fill()
  }

  const vLineCount = 3 + Math.floor(rng() * 5)
  for (let i = 0; i < vLineCount; i++) {
    const x = rng() * w
    ctx.strokeStyle = c.textureColor(rng, 0.02, 0.06)
    ctx.lineWidth = 1
    ctx.beginPath()
    ctx.moveTo(x, 0)
    ctx.lineTo(x, h)
    ctx.stroke()
  }
}

// ===== 拼图形状 =====
function createPuzzlePath(x: number, y: number, w: number, h: number): Path2D {
  const r = 6
  const bump = 7
  const path = new Path2D()
  path.moveTo(x + r, y)
  path.lineTo(x + w - r, y)
  path.arc(x + w - r, y + r, r, -Math.PI / 2, 0)
  path.lineTo(x + w, y + h / 2 - bump)
  path.arc(x + w, y + h / 2, bump, -Math.PI / 2, Math.PI / 2, true)
  path.lineTo(x + w, y + h - r)
  path.arc(x + w - r, y + h - r, r, 0, Math.PI / 2)
  path.lineTo(x + r, y + h)
  path.arc(x + r, y + h - r, r, Math.PI / 2, Math.PI)
  path.lineTo(x, y + h / 2 + bump)
  path.arc(x, y + h / 2, bump, Math.PI / 2, -Math.PI / 2, true)
  path.lineTo(x, y + r)
  path.arc(x + r, y + r, r, Math.PI, -Math.PI / 2)
  path.closePath()
  return path
}

// ===== 工具函数 =====
function seedRandom(seed: number) {
  let s = seed
  return () => {
    s = (s * 16807) % 2147483647
    return (s - 1) / 2147483646
  }
}

function getEventX(e: MouseEvent | TouchEvent): number {
  if ('touches' in e) return e.touches[0].clientX
  return (e as MouseEvent).clientX
}

// ===== 拖拽处理 =====
function onDragStart(e: MouseEvent | TouchEvent) {
  if (status.value === 'success' || status.value === 'verifying') return
  dragging = true
  status.value = 'sliding'
  dragStartX = getEventX(e)
  dragStartLeft = sliderLeft.value
  trackStartTime = Date.now()
  trackData.value = [{ x: sliderLeft.value, y: 0, timestamp: trackStartTime }]
}

function onMouseMove(e: MouseEvent) {
  if (!dragging) return
  handleMove(getEventX(e))
}

function onTouchMove(e: TouchEvent) {
  if (!dragging) return
  e.preventDefault()
  handleMove(getEventX(e))
}

function handleMove(clientX: number) {
  const trackEl = trackRef.value
  if (!trackEl) return

  const delta = clientX - dragStartX
  let newLeft = dragStartLeft + delta
  const maxLeft = captchaData.bgWidth - captchaData.puzzleWidth
  newLeft = Math.max(0, Math.min(newLeft, maxLeft))

  sliderLeft.value = newLeft
  sliderBtnLeft.value = newLeft
  puzzleLeft.value = newLeft

  trackData.value.push({ x: Math.round(newLeft), y: 0, timestamp: Date.now() })
}

function onMouseUp() {
  if (!dragging) return
  dragging = false
  verifySlide()
}

function onTouchEnd() {
  if (!dragging) return
  dragging = false
  verifySlide()
}

// ===== 验证 =====
async function verifySlide() {
  status.value = 'verifying'
  try {
    await captchaApi.verify({
      captchaId: captchaData.captchaId,
      sliderOffset: Math.round(sliderLeft.value),
      trackData: trackData.value
    })
    status.value = 'success'
    puzzleLeft.value = captchaData.targetX
    sliderBtnLeft.value = captchaData.targetX
    sliderLeft.value = captchaData.targetX
    emit('verified', captchaData.captchaId)
  } catch {
    status.value = 'failed'
    puzzleLeft.value = 0
    sliderBtnLeft.value = 0
    sliderLeft.value = 0
    trackData.value = []
  }
}

function refresh() {
  fetchCaptcha()
}

defineExpose({ refresh })
</script>

<style scoped>
/* ===== 公共 ===== */
.slider-captcha {
  user-select: none;
  -webkit-user-select: none;
}

.captcha-canvas-wrapper {
  position: relative;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 0;
}
.captcha-canvas-wrapper > canvas:first-child {
  display: block;
  width: 100%;
  height: 100%;
}

.puzzle-overlay {
  position: absolute;
  pointer-events: none;
  z-index: 2;
  transition: none;
  display: block;
}

/* 状态遮罩 */
.captcha-mask {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 600;
  z-index: 5;
  cursor: pointer;
}
.captcha-error {
  background: rgba(239, 68, 68, 0.12);
  color: #ef4444;
}
.captcha-ok {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
  pointer-events: none;
}

/* 滑块轨道 */
.slider-track {
  position: relative;
  height: 34px;
  border-radius: 17px;
  margin-top: 10px;
  overflow: hidden;
}
.slider-progress {
  position: absolute;
  top: 0;
  left: 0;
  height: 100%;
  border-radius: 17px 0 0 17px;
  transition: none;
}
.slider-btn {
  position: absolute;
  top: -1px;
  height: 36px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: grab;
  z-index: 3;
  transition: none;
}
.slider-btn:active { cursor: grabbing; }
.slider-icon {
  font-size: 16px;
  line-height: 1;
}

.slider-hint {
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  font-size: 12px;
  pointer-events: none;
  white-space: nowrap;
}

/* ===== 浅色主题 ===== */
.slider-captcha.light .captcha-canvas-wrapper {
  background: #eef2f7;
}

.slider-captcha.light .slider-track {
  background: #e4e7ed;
}
.slider-captcha.light .slider-progress {
  background: linear-gradient(90deg, rgba(6, 182, 212, 0.25), rgba(6, 182, 212, 0.35));
}
.slider-captcha.light .slider-btn {
  background: #fff;
  border: 2px solid #d0d5dd;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.slider-captcha.light .slider-btn.sliding {
  border-color: #06b6d4;
  box-shadow: 0 2px 12px rgba(6, 182, 212, 0.3);
}
.slider-captcha.light .slider-btn.verified {
  border-color: #22c55e;
  background: #f0fdf4;
}
.slider-captcha.light .slider-icon {
  color: #64748b;
}
.slider-captcha.light .slider-btn.sliding .slider-icon {
  color: #06b6d4;
}
.slider-captcha.light .slider-btn.verified .slider-icon {
  color: #22c55e;
}
.slider-captcha.light .slider-hint {
  color: #94a3b8;
}

/* ===== 深色主题 ===== */
.slider-captcha.dark .captcha-canvas-wrapper {
  background: #0a1628;
}

.slider-captcha.dark .slider-track {
  background: #0d1f33;
}
.slider-captcha.dark .slider-progress {
  background: linear-gradient(90deg, rgba(6, 182, 212, 0.1), rgba(6, 182, 212, 0.18));
}
.slider-captcha.dark .slider-btn {
  background: #0f2740;
  border: 1.5px solid #1e3a5f;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}
.slider-captcha.dark .slider-btn.sliding {
  border-color: #06b6d4;
  box-shadow: 0 0 12px rgba(6, 182, 212, 0.25);
}
.slider-captcha.dark .slider-btn.verified {
  border-color: rgba(34, 197, 94, 0.5);
  background: rgba(34, 197, 94, 0.08);
}
.slider-captcha.dark .slider-icon {
  color: rgba(255, 255, 255, 0.4);
}
.slider-captcha.dark .slider-btn.sliding .slider-icon {
  color: #06b6d4;
}
.slider-captcha.dark .slider-btn.verified .slider-icon {
  color: #22c55e;
}
.slider-captcha.dark .slider-hint {
  color: rgba(255, 255, 255, 0.25);
}

.slider-captcha.dark .captcha-error {
  background: rgba(239, 68, 68, 0.1);
}
.slider-captcha.dark .captcha-ok {
  background: rgba(34, 197, 94, 0.08);
}
</style>
