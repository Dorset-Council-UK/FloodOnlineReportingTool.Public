const path = require('path');
const { globSync } = require('glob');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CopyPlugin = require('copy-webpack-plugin');

// Dynamically load all *.razor.ts files under the Components folder
const componentEntries = Object.fromEntries(
    globSync('./Components/**/*.razor.ts', { withFileTypes: true })
        .map(o => [
            `components/${o.name.replace(/\.razor\.ts$/, '').toLowerCase()}`,
            `./${o.relativePosix()}`
        ]
    )
);

// Get the base path from app settings, if available
const appSettings = require(process.env.NODE_ENV === 'production' ? './appsettings.json' : './appsettings.Development.json');
const pathBase = appSettings?.GIS?.PathBase || '';

module.exports = {
    entry: {
        app: './Scripts/app.ts',
        ...componentEntries
    },
    resolve: {
        extensions: ['.ts', '.js']
    },
    experiments: {
        outputModule: true,
    },
    output: {
        filename: 'js/[name].js',
        path: path.resolve(__dirname, 'wwwroot'),
        library: {
            type: 'module'
        },
        clean: {
            keep: /^(css\/.*\.css)$/ // Keep and any css files under the css folder
        }
    },
    module: {
        rules: [
            {
                test: /\.([cm]?ts|tsx)$/,
                loader: 'ts-loader',
                exclude: /node_modules/,
            },
            {
                test: /\.s[ac]ss$/i,
                use: [
                    MiniCssExtractPlugin.loader, // Extracts CSS into separate files
                    {
                        loader: 'css-loader', // Creates `style` nodes from JS strings
                        options: {
                            url: false // This tells css-loader not to resolve url() paths
                        }
                    },
                    {
                        loader: 'sass-loader', // Compiles Sass to CSS
                        options: {
                            sassOptions: {
                                quietDeps: true,
                                includePaths: ['node_modules']
                            }
                        }
                    }
                ]
            },
            {
                test: /\.css$/i, // Extracts imported CSS in TypeScript into separate files
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader', // Creates `style` nodes from JS strings
                        options: {
                            url: false // This tells css-loader not to resolve url() paths
                        }
                    },
                ]
            }
        ]
    },
    plugins: [
        // Copy the assets to the output folder for GDS gov frontend, leaflet and favicons
        new CopyPlugin({
            patterns: [
                { from: 'node_modules/leaflet/dist/leaflet.css', to: 'css' },
                { from: 'node_modules/leaflet/dist/images', to: 'css/images' },
                {
                    from: 'Scripts/favicons',
                    to: 'favicons',
                    globOptions: {
                        ignore: ['**/*.template.json']
                    }
                },
                {
                    from: 'Scripts/favicons/site.webmanifest.template.json',
                    to: 'favicons/site.webmanifest',
                    transform(content, absoluteFrom) {
                        return content.toString().replace(/__MANIFEST_PATH_BASE__/g, pathBase);
                    }
                },
                { from: 'Scripts/images/logos', to: 'images/logos' },
            ]
        }),
        // Extract the CSS into a separate file
        new MiniCssExtractPlugin({
            filename: 'css/[name].css'
        })
    ]
}