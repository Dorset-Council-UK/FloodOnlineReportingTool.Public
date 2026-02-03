import Bun from "bun";
import path from "path";
import { compile, compileAsync } from "sass";
import { rmdir, stat } from "node:fs/promises";

const isDev = process.argv.includes("--dev");
//const isWatch = process.argv.includes("--watch");

async function build() {
	console.log(`Building in ${isDev ? "development" : "production"} mode...`);

	await cleanupOldBundles();
	await compileSass();
	await copyAssets();
	await buildBlazorComponents();
	await buildScripts();

	console.log("✅ Build complete!");
}

//async function getAppSettings(): Promise<any | null> {
//	const appSettingsPath = isDev ? "./appsettings.Development.json" : "./appsettings.json";
//	const file = Bun.file(appSettingsPath);

//	if (!(await file.exists())) {
//		console.error(`Warning: App settings file not found at ${appSettingsPath}.`);
//		return null;
//	}

//	return await file.json();
//}

//async function copyAssets() {
//	console.log("📦 Copying assets...");
//	mkdirSync("./wwwroot/css/images", { recursive: true });
//	mkdirSync("./wwwroot/favicons", { recursive: true });
//	mkdirSync("./wwwroot/images", { recursive: true });

//	// Copy MapLibre GL CSS
//	copyFileSync("./node_modules/maplibre-gl/dist/maplibre-gl.css", "./wwwroot/css/maplibre-gl.css");

//	// Copy favicons (excluding template)
//	const appSettings = await getAppSettings();
//    const pathBase = appSettings?.GIS?.PathBase || "";
//	if (existsSync("./Scripts/favicons")) {
//		const faviconFiles = readdirSync("./Scripts/favicons");
//		for (const file of faviconFiles) {
//			if (file === "site.webmanifest.template.json") continue;
//			const srcPath = path.join("./Scripts/favicons", file);
//			if (statSync(srcPath).isFile()) {
//				copyFileSync(srcPath, path.join("./wwwroot/favicons", file));
//			}
//		}
//		// Transform site.webmanifest template with path base
//		const templatePath = "./Scripts/favicons/site.webmanifest.template.json";
//		if (existsSync(templatePath)) {
//			const template = readFileSync(templatePath, "utf-8");
//			const transformed = template.replace(/__MANIFEST_PATH_BASE__/g, pathBase);
//			writeFileSync("./wwwroot/favicons/site.webmanifest", transformed);
//		}
//	}

//	// Copy logo images
//	copyDirectory("./Scripts/images", "./wwwroot/images");
//}

async function cleanupOldBundles() {
	console.log("🧹 Cleaning up old bundles...");

    const directories = ["./wwwroot/css", "./wwwroot/images", "./wwwroot/js"];

	for await (const directory of directories) {
		if (await isDirectory(directory)) {
			await rmdir(directory, { recursive: true });
		}
	}
}

async function isDirectory(path: string): Promise<boolean> {
	try {
		const stats = await stat(path);
		return stats.isDirectory();
	} catch {
		return false;
	}
}

async function copyDirectory(sourceDirectory: string, destinationDirectory: string) {
	const glob = new Bun.Glob(`${sourceDirectory}/**/*.*`);
	for await (const file of glob.scan(".")) {
		const relative = path.relative(sourceDirectory, file);
		const newPath = path.join(destinationDirectory, relative);
        await Bun.write(Bun.file(newPath), Bun.file(file));
	}
}

async function compileSass() {
	console.log("🎨 Compiling SCSS...");

	const name = "app";
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
			Bun.write(Bun.file(to), `${result.css}\n/*# sourceMappingURL=${name}.css.map */\n`);
			Bun.write(Bun.file(`${to}.map`), JSON.stringify(result.sourceMap));
		} else {
			Bun.write(Bun.file(to), result.css);
		}
	}
}

async function copyAssets() {
	console.log("📦 Copying assets...");

	// Copy MapLibre GL CSS
	await Bun.write(Bun.file("./wwwroot/css/maplibre-gl.css"), Bun.file("./node_modules/maplibre-gl/dist/maplibre-gl.css"));

	// Favicons
	await copyDirectory("./Scripts/favicons", "./wwwroot/favicons");

	// Images
    await copyDirectory("./Scripts/images", "./wwwroot/images");
}

async function buildBlazorComponents() {
	// Dynamically get all component typescript entry points
	const entryPoints: string[] = [];
	const glob = new Bun.Glob("./Components/**/*.razor.ts");
	for await (const file of glob.scan(".")) {
		entryPoints.push(file);
	}

	if (entryPoints.length > 0) {
		console.log("📦 Building Blazor components...");
		await Bun.build({
			entrypoints: entryPoints,
			outdir: "./wwwroot/js/components",
			sourcemap: isDev ? "linked" : "none",
            minify: !isDev,
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

//if (isWatch) {
//	console.log("👀 Watching for changes...");
//	const { watch } = await import("fs");
//	const scriptsPath = path.resolve("./Scripts");
//	const componentsPath = path.resolve("./Components");

//	const handleChange = async (filename: string | null) => {
//		if (filename?.endsWith(".ts") || filename?.endsWith(".scss")) {
//			console.log(`\n📝 ${filename} changed, rebuilding...`);
//			await build();
//		}
//	};

//	watch(scriptsPath, { recursive: true }, async (event, filename) => {
//		await handleChange(filename as string | null);
//	});
//	watch(componentsPath, { recursive: true }, async (event, filename) => {
//		await handleChange(filename as string | null);
//	});

//	await build();
//} else {
//	await build();
//}
