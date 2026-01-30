/* eslint-disable no-console */
import { BuildConfig } from "bun";
import path from "path";
import { globSync } from "glob";
import * as sass from "sass";
import { copyFileSync, mkdirSync, readFileSync, writeFileSync, existsSync, readdirSync, statSync } from "fs";

const isDev = process.argv.includes("--dev");
const isWatch = process.argv.includes("--watch");

// Get the base path from app settings, if available
const appSettingsPath = isDev ? "./appsettings.Development.json" : "./appsettings.json";
const appSettingsFile = existsSync(appSettingsPath) ? appSettingsPath : "./appsettings.json";
let pathBase = "";
try {
	const appOptions = JSON.parse(readFileSync(appSettingsFile, "utf-8"));
	pathBase = appOptions?.GIS?.PathBase || "";
} catch (error) {
	console.error(`Failed to read or parse app settings from ${appSettingsFile}:`, error);
}

// Dynamically load all *.razor.ts files under the Components folder
const componentEntries = globSync("./Components/**/*.razor.ts").map((file) => ({
	entrypoint: file,
	outName: path.basename(file, ".razor.ts").toLowerCase(),
}));

const commonConfig: Partial<BuildConfig> = {
	minify: !isDev,
	sourcemap: isDev ? "inline" : "external",
	target: "browser",
};

// Main app bundle
const appBundle: BuildConfig = {
	...commonConfig,
	entrypoints: ["./Scripts/app.ts"],
	outdir: "./wwwroot/js",
	naming: "[dir]/app.[ext]",
};

// Component bundles - dynamically generated from *.razor.ts files
const componentBundles: BuildConfig[] = componentEntries.map(({ entrypoint, outName }) => ({
	...commonConfig,
	entrypoints: [entrypoint],
	outdir: "./wwwroot/js/components",
	naming: `[dir]/${outName}.[ext]`,
}));

function copyDirectory(src: string, dest: string) {
	if (!existsSync(src)) return;
	mkdirSync(dest, { recursive: true });
	const entries = readdirSync(src);
	for (const entry of entries) {
		const srcPath = path.join(src, entry);
		const destPath = path.join(dest, entry);
		if (statSync(srcPath).isDirectory()) {
			copyDirectory(srcPath, destPath);
		} else {
			copyFileSync(srcPath, destPath);
		}
	}
}

function copyAssets() {
	console.log("📦 Copying assets...");
	mkdirSync("./wwwroot/css/images", { recursive: true });
	mkdirSync("./wwwroot/favicons", { recursive: true });
	mkdirSync("./wwwroot/images", { recursive: true });

	// Copy MapLibre GL CSS
	copyFileSync("./node_modules/maplibre-gl/dist/maplibre-gl.css", "./wwwroot/css/maplibre-gl.css");

	// Copy favicons (excluding template)
	if (existsSync("./Scripts/favicons")) {
		const faviconFiles = readdirSync("./Scripts/favicons");
		for (const file of faviconFiles) {
			if (file === "site.webmanifest.template.json") continue;
			const srcPath = path.join("./Scripts/favicons", file);
			if (statSync(srcPath).isFile()) {
				copyFileSync(srcPath, path.join("./wwwroot/favicons", file));
			}
		}
		// Transform site.webmanifest template with path base
		const templatePath = "./Scripts/favicons/site.webmanifest.template.json";
		if (existsSync(templatePath)) {
			const template = readFileSync(templatePath, "utf-8");
			const transformed = template.replace(/__MANIFEST_PATH_BASE__/g, pathBase);
			writeFileSync("./wwwroot/favicons/site.webmanifest", transformed);
		}
	}

	// Copy logo images
	copyDirectory("./Scripts/images", "./wwwroot/images");
}

function compileSass() {
	console.log("🎨 Compiling SCSS...");
	mkdirSync("./wwwroot/css", { recursive: true });

	const scssPath = "./Scripts/app.scss";
	if (existsSync(scssPath)) {
		const result = sass.compile(scssPath, {
			style: isDev ? "expanded" : "compressed",
			sourceMap: isDev,
			loadPaths: ["node_modules"],
			quietDeps: true,
		});
		writeFileSync("./wwwroot/css/app.css", result.css);
		if (isDev && result.sourceMap) {
			writeFileSync("./wwwroot/css/app.css.map", JSON.stringify(result.sourceMap));
		}
	}
}

async function build() {
	console.log(`Building in ${isDev ? "development" : "production"} mode...`);
	copyAssets();
	compileSass();

	const allBundles = [appBundle, ...componentBundles];
	const results = await Promise.all(allBundles.map((bundle) => Bun.build(bundle)));

	for (const result of results) {
		if (!result.success) {
			console.error("Build failed:");
			for (const log of result.logs) {
				console.error(log);
			}
			process.exit(1);
		}
	}
	console.log("✅ Build complete!");
}

if (isWatch) {
	console.log("👀 Watching for changes...");
	const { watch } = await import("fs");
	const scriptsPath = path.resolve("./Scripts");
	const componentsPath = path.resolve("./Components");

	const handleChange = async (filename: string | null) => {
		if (filename?.endsWith(".ts") || filename?.endsWith(".scss")) {
			console.log(`\n📝 ${filename} changed, rebuilding...`);
			await build();
		}
	};

	watch(scriptsPath, { recursive: true }, async (event, filename) => {
		await handleChange(filename as string | null);
	});
	watch(componentsPath, { recursive: true }, async (event, filename) => {
		await handleChange(filename as string | null);
	});

	await build();
} else {
	await build();
}