
    export type RemoteKeys = 'app2/Form';
    type PackageType<T> = T extends 'app2/Form' ? typeof import('app2/Form') :any;