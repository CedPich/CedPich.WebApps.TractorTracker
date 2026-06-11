<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import Map from 'ol/Map'
import View from 'ol/View'
import TileLayer from 'ol/layer/Tile'
import VectorLayer from 'ol/layer/Vector'
import VectorSource from 'ol/source/Vector'
import OSM from 'ol/source/OSM'
import XYZ from 'ol/source/XYZ'
import Feature from 'ol/Feature'
import Point from 'ol/geom/Point'
import LineString from 'ol/geom/LineString'
import { fromLonLat } from 'ol/proj'
import { Circle as CircleStyle, Fill, Icon, Stroke, Style } from 'ol/style'
import type { PositionDto } from '@/api/machineApi'

const MAPBOX_TOKEN = import.meta.env.VITE_MAPBOX_TOKEN ?? ''
const MAPBOX_XYZ_URL = `https://api.mapbox.com/styles/v1/noneofus/cj79irz8w86xm2qtkehmw9r6b/tiles/256/{z}/{x}/{y}?access_token=${MAPBOX_TOKEN}`

const props = defineProps<{
  currentPosition: PositionDto | null
  history: PositionDto[]
  live?: boolean
}>()

const mapEl = ref<HTMLDivElement>()
const basemap = ref<'osm' | 'mapbox'>('mapbox')
const popup = ref<{
  time: string
  lat: string
  lng: string
  speed: string | null
  heading: string | null
  altitude: string | null
  satellites: number | null
  address: string | null
} | null>(null)
const popupStyle = ref({ top: '0px', left: '0px' })

let map: Map
let baseLayer: TileLayer
let vectorSource: VectorSource

function buildMapboxSource() {
  return new XYZ({ url: MAPBOX_XYZ_URL, tileSize: 256, crossOrigin: 'anonymous' })
}

function toggleBasemap() {
  basemap.value = basemap.value === 'osm' ? 'mapbox' : 'osm'
  baseLayer.setSource(basemap.value === 'osm' ? new OSM() : buildMapboxSource())
}

// Icône tracteur SVG
const tractorSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="40" height="40">
  <circle cx="20" cy="44" r="14" fill="none" stroke="#4ade80" stroke-width="3"/>
  <circle cx="20" cy="44" r="5" fill="#4ade80"/>
  <circle cx="50" cy="48" r="8" fill="none" stroke="#4ade80" stroke-width="3"/>
  <circle cx="50" cy="48" r="3" fill="#4ade80"/>
  <rect x="18" y="34" width="34" height="10" rx="2" fill="#16a34a"/>
  <rect x="30" y="20" width="20" height="16" rx="2" fill="#15803d"/>
  <rect x="33" y="23" width="14" height="9" rx="1" fill="#111827" opacity="0.8"/>
  <rect x="14" y="28" width="18" height="8" rx="2" fill="#166534"/>
  <rect x="24" y="18" width="3" height="10" rx="1" fill="#4ade80"/>
</svg>`
const tractorIcon = new Style({
  image: new Icon({ src: `data:image/svg+xml;utf8,${encodeURIComponent(tractorSvg)}`, anchor: [0.5, 0.85] }),
})

const historyPointStyle = new Style({
  image: new CircleStyle({ radius: 4, fill: new Fill({ color: '#3b82f6' }), stroke: new Stroke({ color: '#1e3a5f', width: 1 }) }),
})
const trackStyle = new Style({ stroke: new Stroke({ color: '#3b82f6', width: 2, lineDash: [4, 4] }) })

onMounted(() => {
  vectorSource = new VectorSource()
  baseLayer = new TileLayer({ source: buildMapboxSource() })

  map = new Map({
    target: mapEl.value,
    layers: [baseLayer, new VectorLayer({ source: vectorSource })],
    view: new View({ center: fromLonLat([2.5, 46.5]), zoom: 6 }),
  })

  map.on('click', (evt) => {
    const feature = map.forEachFeatureAtPixel(evt.pixel, f => f)
    if (feature) {
      const data = feature.get('pointData') as PositionDto | undefined
      if (data) {
        popup.value = {
          time: new Date(data.recordedAt).toLocaleString('fr-FR'),
          lat: data.latitude.toFixed(6),
          lng: data.longitude.toFixed(6),
          speed: data.speedKmh != null ? `${data.speedKmh.toFixed(1)} km/h` : null,
          heading: data.headingDegrees != null ? `${Math.round(data.headingDegrees)}°` : null,
          altitude: data.altitudeMeters != null ? `${Math.round(data.altitudeMeters)} m` : null,
          satellites: data.satellites ?? null,
          address: data.formattedAddress ?? null,
        }
        popupStyle.value = { top: `${(evt.pixel[1] ?? 0) + 12}px`, left: `${(evt.pixel[0] ?? 0) + 12}px` }
        return
      }
    }
    popup.value = null
  })

  updateFeatures()
})

onUnmounted(() => map?.setTarget(undefined))

// En mode live : suivi de position sans changer le zoom
watch(() => props.currentPosition, (pos) => {
  if (!props.live || !pos || !map) return
  map.getView().animate({
    center: fromLonLat([pos.longitude, pos.latitude]),
    duration: 400,
  })
})

watch(() => [props.history], updateFeatures, { deep: true })

function updateFeatures() {
  if (!vectorSource) return
  vectorSource.clear()
  popup.value = null

  if (props.history.length > 1) {
    const coords = props.history.map(p => fromLonLat([p.longitude, p.latitude]))
    const line = new Feature(new LineString(coords))
    line.setStyle(trackStyle)
    vectorSource.addFeature(line)

    for (const p of props.history) {
      const f = new Feature(new Point(fromLonLat([p.longitude, p.latitude])))
      f.setStyle(historyPointStyle)
      f.set('pointData', p)
      vectorSource.addFeature(f)
    }
  }

  if (props.currentPosition) {
    const coord = fromLonLat([props.currentPosition.longitude, props.currentPosition.latitude])
    const tractor = new Feature(new Point(coord))
    tractor.setStyle(tractorIcon)
    tractor.set('pointData', props.currentPosition)
    vectorSource.addFeature(tractor)
  }

  if (!props.live) {
    const extent = vectorSource.getExtent()
    if (isFinite(extent[0]))
      map.getView().fit(extent, { padding: [48, 48, 48, 48], maxZoom: 17, duration: 500 })
  }
}

function zoomToTractor() {
  if (!props.currentPosition) return
  map.getView().animate({
    center: fromLonLat([props.currentPosition.longitude, props.currentPosition.latitude]),
    zoom: 17,
    duration: 400,
  })
}

function zoomToExtent() {
  const extent = vectorSource.getExtent()
  if (isFinite(extent[0]))
    map.getView().fit(extent, { padding: [48, 48, 48, 48], maxZoom: 17, duration: 400 })
}
</script>

<template>
  <div style="position:relative; width:100%; height:100%;">
    <div ref="mapEl" class="tracker-map" />
    <div v-if="popup" class="map-popup" :style="popupStyle">
      <div class="popup-time">{{ popup.time }}</div>
      <div class="popup-coords">{{ popup.lat }}, {{ popup.lng }}</div>
      <div v-if="popup.address" class="popup-address">{{ popup.address }}</div>
      <div class="popup-meta">
        <span v-if="popup.altitude">⛰ {{ popup.altitude }}</span>
        <span v-if="popup.speed">⚡ {{ popup.speed }}</span>
        <span v-if="popup.heading">🧭 {{ popup.heading }}</span>
        <span v-if="popup.satellites != null">🛰 {{ popup.satellites }}</span>
      </div>
    </div>
    <div class="map-controls">
      <button class="map-btn" :disabled="!currentPosition" title="Centrer sur le tracteur" @click="zoomToTractor">🚜</button>
      <button class="map-btn" title="Zoom sur l'emprise" @click="zoomToExtent">⛶</button>
      <button class="map-btn basemap-btn" :class="{ active: basemap === 'mapbox' }" title="Changer de fond de carte" @click="toggleBasemap">
        {{ basemap === 'osm' ? 'Sat' : 'OSM' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.tracker-map { width: 100%; height: 100%; min-height: 400px; }

.map-controls {
  position: absolute;
  bottom: 0.75rem;
  right: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  z-index: 10;
}
.map-btn {
  width: 2rem;
  height: 2rem;
  background: rgba(17, 24, 39, 0.88);
  border: 1px solid #374151;
  border-radius: 4px;
  color: #f3f4f6;
  font-size: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
}
.map-btn:hover:not(:disabled) { background: rgba(55, 65, 81, 0.95); }
.map-btn:disabled { opacity: 0.35; cursor: default; }
.basemap-btn { font-size: 0.65rem; font-weight: 700; letter-spacing: 0.03em; }
.basemap-btn.active { border-color: #3b82f6; color: #93c5fd; }

.map-popup {
  position: absolute;
  background: rgba(17, 24, 39, 0.95);
  border: 1px solid #374151;
  border-radius: 6px;
  padding: 0.4rem 0.7rem;
  pointer-events: none;
  z-index: 10;
  white-space: nowrap;
}
.popup-time { font-size: 0.85rem; color: #f3f4f6; font-weight: 500; }
.popup-coords { font-size: 0.75rem; color: #9ca3af; margin-top: 2px; }
.popup-address { font-size: 0.75rem; color: #d1d5db; margin-top: 4px; max-width: 240px; white-space: normal; line-height: 1.3; }
.popup-meta { display: flex; gap: 0.6rem; flex-wrap: wrap; margin-top: 4px; font-size: 0.75rem; color: #9ca3af; }
</style>
