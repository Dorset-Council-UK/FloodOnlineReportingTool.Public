import * as L from "leaflet";
import proj4 from 'proj4';
import "proj4leaflet";

let map: L.Map | null = null;
let helper: any = null;
let markerEnabled: boolean = false;
let marker: L.Marker | null = null;

/**
 * Settup a simple map using the OS Maps API and Leaflet, using the British National Grid projection.
 * @see https://labs.os.uk/public/os-data-hub-examples/os-maps-api/zxy-27700-basic-map#leaflet
 */
export function setupMap(element: HTMLElement, centreEasting: Number, centreNorthing: Number, startingEasting: Number | undefined, startingNorthing: Number | undefined) {
    destroyMap();

    // Setup the EPSG:27700 (British National Grid) projection.
    const resolutions: number[] = [896.0, 448.0, 224.0, 112.0, 56.0, 28.0, 14.0, 7.0, 3.5, 1.75];
    const EPSG27700: L.CRS = new L.Proj.CRS('EPSG:27700', '+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +ellps=airy +towgs84=446.448,-125.157,542.06,0.15,0.247,0.842,-20.489 +units=m +no_defs', {
        resolutions: resolutions,
        origin: [-238375.0, 1376256.0]
    });

    const [lat, lng] = transformCoords([centreEasting, centreNorthing]);
    const centre = new L.LatLng(lat, lng);

    const mapOptions: L.MapOptions = {
        crs: EPSG27700,
        minZoom: 0,
        maxZoom: resolutions.length - 1,
        center: centre,
        zoom: 4,
        maxBounds: [
            transformCoords([-238375.0, 0.0]),
            transformCoords([900000.0, 1376256.0])
        ],
        attributionControl: false
    };

    map = L.map(element, mapOptions);

    if (startingEasting && startingNorthing) {
        const [startingLat, startingLng] = transformCoords([startingEasting, startingNorthing]);
        setMarkerLocation(new L.LatLng(startingLat, startingLng), map);
    }

    const apiKey = 'J3H6E7O9J3cZuUvkjdOASdbGDAmQxjZJ';
    L.tileLayer(`https://api.os.uk/maps/raster/v1/zxy/Road_27700/{z}/{x}/{y}.png?key=${apiKey}`).addTo(map);
    map.on('click', onMapClick);
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
 */
function transformCoords(arr: any) {
    return proj4('EPSG:27700', 'EPSG:4326', arr).reverse();
};

/**
 * When the user clicks on the map, set the marker location and remember the coordinates.
 */
function onMapClick(evt: L.LeafletMouseEvent) {
    if (!markerEnabled) return;

    const latLng = evt.latlng as L.LatLng;
    setMarkerLocation(latLng, evt.target);
    rememberCoordinates(latLng);
    markerEnabled = false;
}

/**
 * Either set the marker location or move the existing marker to the new location.
 * @see https://leafletjs.com/reference.html#marker
 */
function setMarkerLocation(latLng: L.LatLng, map: L.Map) {
    if (marker) {
        marker.setLatLng(latLng);
        return;
    }

    const options: L.MarkerOptions = {
        alt: 'Chosen location',
        riseOnHover: true,
        draggable: true
    };
    marker = L.marker(latLng, options).addTo(map);
    marker.on('dragend', onMarkerDragEnd);
}

function rememberCoordinates(latLng: L.LatLng) {
    if (!helper) {
        console.error('Helper not set, unable to update new coordinates!');
        return;
    }

    // Transform coordinates from WGS84 (EPSG:4326) to British National Grid (EPSG:27700).
    const [easting, northing] = proj4('EPSG:4326', 'EPSG:27700', [latLng.lng, latLng.lat]);
    helper.invokeMethodAsync("CoordinatesChanged", easting, northing);
}

/**
 * When the user drags the marker, save the new location.
 */
function onMarkerDragEnd(evt: L.DragEndEvent) {
    const latLng = evt.target.getLatLng() as L.LatLng;
    rememberCoordinates(latLng);
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