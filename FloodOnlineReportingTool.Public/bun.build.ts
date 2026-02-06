import Bun from "bun";
import { watch } from "node:fs"
import { basename, relative, resolve } from "node:path";
import { compile } from "sass";
import clean from "./bun.clean";

const isDev = process.argv.includes("--dev");
const isWatch = process.argv.includes("--watch");

async function build() {
	console.log(`Building in ${isDev ? "development" : "production"} mode...`);

	await clean();
	await compileSass();
	await copyAssets();
	await buildBlazorComponents();
	await buildScripts();

	console.log("✅ Build complete!");
}

async function copyDirectory(sourceDirectory: string, destinationDirectory: string) {
	const glob = new Bun.Glob(`${sourceDirectory}/**/*.*`);
	for await (const filePath of glob.scan(".")) {
		const relativePath = relative(sourceDirectory, filePath);
		const to = resolve(destinationDirectory, relativePath);
		await Bun.write(to, Bun.file(filePath));
	}
}

async function copyFaviconsDirectory(sourceDirectory: string, destinationDirectory: string) {
	const glob = new Bun.Glob(`${sourceDirectory}/**/*.*`);
	for await (const filePath of glob.scan(".")) {
		const relativePath = relative(sourceDirectory, filePath);

		if (filePath.endsWith("site.webmanifest.template.json")) {
			const newFilename = basename(relativePath, ".template.json");
			const to = resolve(destinationDirectory, newFilename);

            // Transform template
			const appSettings = await getAppSettings();
			const pathBase = appSettings?.GIS?.PathBase || undefined;
			if (pathBase) {
				const templateContent = await Bun.file(filePath).text();
				const transformed = templateContent.replace(/__MANIFEST_PATH_BASE__/g, pathBase);
				await Bun.write(to, transformed);
			} else {
                console.error("Error: PathBase not found in app settings, cannot transform site.webmanifest.template.json.");
				await Bun.write(to, Bun.file(filePath));
			}
		} else {
			const to = resolve(destinationDirectory, relativePath);
			await Bun.write(to, Bun.file(filePath));
		}
	}
}

async function getAppSettings(): Promise<any | undefined> {
	const appSettingsPath = isDev ? "./appsettings.Development.json" : "./appsettings.json";
	const file = Bun.file(appSettingsPath);

	if (!(await file.exists())) {
		console.error(`Error: App settings file not found at ${appSettingsPath}.`);
		return undefined;
	}

	return await file.json();
}

async function compileSass() {
	console.log("🎨 Compiling SCSS...");

	const names = ["app"];

	for (const name of names) {
		const from = `./Scripts/${name}.scss`;
		const to = `./wwwroot/css/${name}.css`;

		const scssFile = Bun.file(from)
		if (await scssFile.exists()) {
			const result = compile(from, {
				style: isDev ? "expanded" : "compressed",
				sourceMap: isDev,
				quietDeps: true,
			});

			if (result.sourceMap) {
				Bun.write(to, `${result.css}\n/*# sourceMappingURL=${name}.css.map */\n`);
				Bun.write(`${to}.map`, JSON.stringify(result.sourceMap));
			} else {
				Bun.write(to, result.css);
			}
		}
	}
}

async function copyAssets() {
	console.log("📦 Copying assets...");

	// Copy MapLibre GL CSS
	await Bun.write("./wwwroot/css/maplibre-gl.css", Bun.file("./node_modules/maplibre-gl/dist/maplibre-gl.css"));

	// Favicons
	await copyFaviconsDirectory("./Scripts/favicons", "./wwwroot/favicons");

	// Images
    await copyDirectory("./Scripts/images", "./wwwroot/images");
}

async function buildBlazorComponents() {
	console.log("📦 Building Blazor components...");

	// Dynamically get all component typescript entry points
	const glob = new Bun.Glob("./Components/**/*.razor.ts");
	for await (const file of glob.scan(".")) {
		const jsName = basename(file, ".razor.ts");
		const outdir = resolve("./wwwroot/js", file, "..");
		await Bun.build({
			entrypoints: [file],
			outdir,
			sourcemap: isDev ? "linked" : "none",
			minify: true,
			naming: `[dir]/${jsName}.[ext]`,
		});
	}
}

async function buildScripts() {
	// Dynamically get all typescript entry points
	const entryPoints: string[] = [];
	const glob = new Bun.Glob("./Scripts/**/*.ts");
	for await (const file of glob.scan(".")) {
		entryPoints.push(file);
	}

	if (entryPoints.length > 0) {
		console.log("📦 Building scripts...");
		await Bun.build({
			entrypoints: entryPoints,
			outdir: "./wwwroot/js",
			sourcemap: isDev ? "linked" : "none",
			minify: !isDev,
		});
	}
}

await build();

if (isWatch) {
	console.log("👀 Watching for changes...");

	// Watch the scripts directory
	watch("./Scripts", { recursive: true }, async (eventType, filename) => {
		if (filename && eventType === "change") {
			if (filename.endsWith(".ts")) {
				console.log(`📝 ${filename} changed, rebuilding...`);
				await buildScripts();
				console.log("✅ Re-build complete!");
			}

			if (filename.endsWith(".scss")) {
				console.log(`📝 ${filename} changed, rebuilding...`);
				await compileSass();
				console.log("✅ Re-build complete!");
			}
		}
	});

	// Watch the blazor components directory
	watch("./Components", { recursive: true }, async (eventType, filename) => {
		if (filename && eventType === "change") {
			if (filename.endsWith(".razor.ts")) {
				console.log(`📝 ${filename} changed, rebuilding...`);
				await buildBlazorComponents();
				console.log("✅ Re-build complete!");
			}
		}
	});
}
