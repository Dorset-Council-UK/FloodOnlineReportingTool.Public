import { rm, stat } from "node:fs/promises";

export default async function clean() {
	console.log("🧹 Cleaning up...");

	const directories = ["./wwwroot/css", "./wwwroot/favicons", "./wwwroot/images", "./wwwroot/js"];

	for (const directory of directories) {
		if (await isDirectory(directory)) {
			try {
				await rm(directory, { recursive: true });
			} catch (err) {
				console.error(`Failed to remove ${directory}:`, err);
			}
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

// Only auto run clean() when executed directly, not when imported
if (import.meta.main) {
	await clean();
}