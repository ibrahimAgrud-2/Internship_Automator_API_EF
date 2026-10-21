import { ref, onBeforeUnmount } from 'vue'

// Birkaç saniye görünüp kendiliğinden kaybolan bildirim (işlem başarılı oldu vb.)
export function useNotice(durationMs = 6000) {
  const notice = ref(null) // { type, text } ya da null
  let timer

  function pushNotice(text, type = 'success') {
    notice.value = { type, text }
    clearTimeout(timer)
    timer = setTimeout(() => {
      notice.value = null
    }, durationMs)
  }

  onBeforeUnmount(() => clearTimeout(timer))

  return { notice, pushNotice }
}
