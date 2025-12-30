import proj4 from "proj4";
import maplibregl, { Map, Marker, LngLat } from "maplibre-gl";

// Define the British National Grid projection for coordinate transformations
proj4.defs(
  "EPSG:27700",
  "+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +ellps=airy +towgs84=446.448,-125.157,542.06,0.15,0.247,0.842,-20.489 +units=m +no_defs",
);

let map: Map | null = null;
let helper: any = null;
let markerEnabled: boolean = false;
let marker: Marker | null = null;

/**
 * Setup a map using the OS Maps Vector Tile Service API and MapLibre GL JS.
 * @see https://osdatahub.os.uk/docs/vts/overview
 */
export function setupMap(
  element: HTMLElement,
  centreEasting: number,
  centreNorthing: number,
  startingEasting: number | undefined,
  startingNorthing: number | undefined,
  apiKey: string,
  osLicenceNumber: string,
) {
  if (apiKey) {
    destroyMap();

    // Transform centre coordinates from BNG to WGS84
    const [centreLng, centreLat] = transformToWGS84(
      centreEasting,
      centreNorthing,
    );

    // OS Maps Vector Tile Service style URL
    // Available styles: Road, Outdoor, Light, Night
    const styleUrl = `https://api.os.uk/maps/vector/v1/vts/resources/styles?srs=3857&key=${apiKey}`;

    map = new maplibregl.Map({
      container: element,
      style: styleUrl,
      center: [centreLng, centreLat],
      zoom: 9,
      maxZoom: 20,
      maxBounds: [
        [-10.76, 49.0], // Southwest coordinates (roughly covers UK)
        [1.9, 61.0], // Northeast coordinates
      ],
      attributionControl: false,
    });

    // Add custom attribution control with OS licence
    map.addControl(
      new maplibregl.AttributionControl({
        customAttribution: `&copy; Crown Copyright and database rights ${new Date().getFullYear()} OS ${osLicenceNumber}`,
      }),
    );

    // Add navigation controls
    map.addControl(new maplibregl.NavigationControl(), "top-right");

    // Wait for map to load before adding marker and click handler
    map.on("load", () => {
      // Add starting marker if coordinates provided
      if (startingEasting && startingNorthing) {
        const [startingLng, startingLat] = transformToWGS84(
          startingEasting,
          startingNorthing,
        );
        setMarkerLocation(new LngLat(startingLng, startingLat));
      }

      // Enable marker placement by default
      turnMarkerOn();

      // Handle map clicks
      map!.on("click", onMapClick);
    });
  }
}

export function setHelper(helperRef: any) {
  helper = helperRef;
}

/**
 * The user has clicked the "Choose location" button, so enable the marker.
 */
export function turnMarkerOn() {
  markerEnabled = true;
}

/**
 * Transform coordinates from British National Grid (EPSG:27700) to WGS84 (EPSG:4326).
 * Returns [longitude, latitude] for MapLibre compatibility.
 */
function transformToWGS84(easting: number, northing: number): [number, number] {
  return proj4("EPSG:27700", "EPSG:4326", [easting, northing]) as [
    number,
    number,
  ];
}

/**
 * Transform coordinates from WGS84 (EPSG:4326) to British National Grid (EPSG:27700).
 * Returns [easting, northing].
 */
function transformToBNG(lng: number, lat: number): [number, number] {
  return proj4("EPSG:4326", "EPSG:27700", [lng, lat]) as [number, number];
}

/**
 * When the user clicks on the map, set the marker location and remember the coordinates.
 */
function onMapClick(evt: maplibregl.MapMouseEvent) {
  if (!markerEnabled) return;

  const lngLat = evt.lngLat;
  setMarkerLocation(lngLat);
  rememberCoordinates(lngLat);
}

/**
 * Either set the marker location or move the existing marker to the new location.
 */
function setMarkerLocation(lngLat: LngLat) {
  if (!map) return;

  if (marker) {
    marker.setLngLat(lngLat);
    return;
  }

  marker = new maplibregl.Marker({
    draggable: true,
    color: "#05476d", // Blue marker color
  })
    .setLngLat(lngLat)
    .addTo(map);

  marker.on("dragend", onMarkerDragEnd);
}

function rememberCoordinates(lngLat: LngLat) {
  if (!helper) {
    console.error("Helper not set, unable to update new coordinates!");
    return;
  }

  // Transform coordinates from WGS84 to British National Grid
  const [easting, northing] = transformToBNG(lngLat.lng, lngLat.lat);
  helper.invokeMethodAsync("CoordinatesChanged", easting, northing);
}

/**
 * When the user drags the marker, save the new location.
 */
function onMarkerDragEnd() {
  if (!marker) return;
  const lngLat = marker.getLngLat();
  rememberCoordinates(lngLat);
}

export function destroyMap() {
  if (marker) {
    marker.remove();
    marker = null;
  }
  markerEnabled = false;
  helper = null;
  if (map) {
    map.remove();
    map = null;
  }
}
