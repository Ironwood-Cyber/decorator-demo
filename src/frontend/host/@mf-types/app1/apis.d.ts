
    export type RemoteKeys = 'app1/Form';
    type PackageType<T> = T extends 'app1/Form' ? typeof import('app1/Form') :any;