/* eslint-disable no-console */
import { BuildConfig } from "bun";
import path from "path";

const isDev = process.argv.includes("--dev");
const isWatch = process.argv.includes("--watch");

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
				naming: "[dir]/bundle.[ext]",
};

async function build() {
				console.log(`Building in ${isDev ? "development" : "production"} mode...`);

				const result = await Bun.build(appBundle);

				if (!result.success) {
					console.error("Build failed:");
					for (const log of result.logs) {
						console.error(log);
					}
					process.exit(1);
				}

				console.log("✅ Build complete!");
}

if (isWatch) {
				console.log("👀 Watching for changes...");
				const { watch } = await import("fs");
				const scriptsPath = path.resolve("./Scripts");

				watch(scriptsPath, { recursive: true }, async (event, filename) => {
					if (filename?.endsWith(".ts") || filename?.endsWith(".scss")) {
						console.log(`\n📝 ${filename} changed, rebuilding...`);
						await build();
					}
				});

				// Initial build
				await build();
} else {
				await build();
}