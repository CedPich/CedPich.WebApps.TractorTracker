<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import Map from 'ol/Map'
import View from 'ol/View'
import TileLayer from 'ol/layer/Tile'
import VectorLayer from 'ol/layer/Vector'
import VectorSource from 'ol/source/Vector'
import OSM from 'ol/source/OSM'
import Feature from 'ol/Feature'
import Point from 'ol/geom/Point'
import LineString from 'ol/geom/LineString'
import { fromLonLat } from 'ol/proj'
import { Circle as CircleStyle, Fill, Stroke, Style } from 'ol/style'
import type { PositionDto } from '@/api/machineApi'

const props = defineProps<{
  currentPosition: PositionDto | null
  history: PositionDto[]
}>()

const mapEl = ref<HTMLDivElement>()
let map: Map
let vectorSource: VectorSource

const currentStyle = new Style({
  image: new CircleStyle({ radius: 8, fill: new Fill({ color: '#ef4444' }), stroke: new Stroke({ color: '#fff', width: 2 }) }),
})
const trackStyle = new Style({ stroke: new Stroke({ color: '#3b82f6', width: 2 }) })

onMounted(() => {
  vectorSource = new VectorSource()
  map = new Map({
    target: mapEl.value,
    layers: [
      new TileLayer({ source: new OSM() }),
      new VectorLayer({ source: vectorSource }),
    ],
    view: new View({ center: fromLonLat([2.5, 46.5]), zoom: 6 }),
  })
  updateFeatures()
})

onUnmounted(() => map?.setTarget(undefined))

watch(() => [props.currentPosition, props.history], updateFeatures, { deep: true })

function updateFeatures() {
  if (!vectorSource) return
  vectorSource.clear()

  if (props.history.length > 1) {
    const coords = props.history.map(p => fromLonLat([p.longitude, p.latitude]))
    const line = new Feature(new LineString(coords))
    line.setStyle(trackStyle)
    vectorSource.addFeature(line)
  }

  if (props.currentPosition) {
    const coord = fromLonLat([props.currentPosition.longitude, props.currentPosition.latitude])
    const point = new Feature(new Point(coord))
    point.setStyle(currentStyle)
    vectorSource.addFeature(point)
    map.getView().animate({ center: coord, zoom: 14, duration: 500 })
  }
}
</script>

<template>
  <div ref="mapEl" class="tracker-map" />
</template>

<style scoped>
.tracker-map { width: 100%; height: 100%; min-height: 400px; }
</style>
