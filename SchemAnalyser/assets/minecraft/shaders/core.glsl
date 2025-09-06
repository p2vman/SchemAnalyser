mat4 mat4FromQat(vec4 q) {
    float xx = q.x * q.x;
    float xy = q.x * q.y;
    float xz = q.x * q.z;
    float xw = q.x * q.w;
    float yy = q.y * q.y;
    float yz = q.y * q.z;
    float yw = q.y * q.w;
    float zz = q.z * q.z;
    float zw = q.z * q.w;

    mat4 RotationMatrix;
    RotationMatrix[0][0] = 1.0 - 2.0 * (yy + zz);
    RotationMatrix[0][1] = 2.0 * (xy - zw);
    RotationMatrix[0][2] = 2.0 * (xz + yw);
    RotationMatrix[0][3] = 0.0;

    RotationMatrix[1][0] = 2.0 * (xy + zw);
    RotationMatrix[1][1] = 1.0 - 2.0 * (xx + zz);
    RotationMatrix[1][2] = 2.0 * (yz - xw);
    RotationMatrix[1][3] = 0.0;

    RotationMatrix[2][0] = 2.0 * (xz - yw);
    RotationMatrix[2][1] = 2.0 * (yz + xw);
    RotationMatrix[2][2] = 1.0 - 2.0 * (xx + yy);
    RotationMatrix[2][3] = 0.0;

    RotationMatrix[3][0] = 0.0;
    RotationMatrix[3][1] = 0.0;
    RotationMatrix[3][2] = 0.0;
    RotationMatrix[3][3] = 1.0;

    return RotationMatrix;
}

float gsin(float b) {
    float h = (b * b) / 128;

    return sin(h) * 128;
}